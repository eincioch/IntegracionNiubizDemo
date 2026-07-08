using System.Globalization;
using System.Text.Json;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Application.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IntegracionNiubizDemo.Infrastructure.Geocoding;

/// <summary>
/// Implementación de <see cref="IGeocodingService"/> que usa la Google Maps Geocoding API
/// para resolver coordenadas de geolocalización del navegador a una dirección postal
/// (calle, ciudad, departamento/estado, país, código postal).
/// </summary>
public class GoogleGeocodingService : IGeocodingService
{
    private readonly HttpClient _http;
    private readonly GoogleMapsOptions _opt;
    private readonly ILogger<GoogleGeocodingService> _logger;

    public GoogleGeocodingService(HttpClient http, IOptions<GoogleMapsOptions> opt, ILogger<GoogleGeocodingService> logger)
    {
        _http = http;
        _opt = opt.Value;
        _logger = logger;
    }

    public async Task<ReverseGeocodeResult> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
        {
            return new ReverseGeocodeResult(false, null, null, null, null, null,
                "Google Maps API key no configurada (GoogleMaps:ApiKey).");
        }

        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lng = longitude.ToString(CultureInfo.InvariantCulture);
        var url = $"{_opt.GeocodeBaseUrl}?latlng={lat},{lng}&language={_opt.Language}&key={_opt.ApiKey}";

        try
        {
            using var res = await _http.GetAsync(url, ct);
            var json = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError("Error en Google Geocoding API: {Status} {Body}", res.StatusCode, json);
                return new ReverseGeocodeResult(false, null, null, null, null, null, "No se pudo consultar el servicio de geocodificación.");
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var status = root.TryGetProperty("status", out var st) ? st.GetString() : null;

            if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Google Geocoding API devolvió estado {Status}: {Body}", status, json);
                return new ReverseGeocodeResult(false, null, null, null, null, null, $"Geocodificación sin resultados (status: {status}).");
            }

            if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                return new ReverseGeocodeResult(false, null, null, null, null, null, "No se encontraron resultados para esa ubicación.");
            }

            var first = results[0];
            string? streetNumber = null, route = null, city = null, state = null, country = null, postalCode = null;

            if (first.TryGetProperty("address_components", out var components))
            {
                foreach (var comp in components.EnumerateArray())
                {
                    if (!comp.TryGetProperty("types", out var typesEl)) continue;
                    var types = typesEl.EnumerateArray().Select(t => t.GetString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var longName = comp.TryGetProperty("long_name", out var ln) ? ln.GetString() : null;
                    var shortName = comp.TryGetProperty("short_name", out var sn) ? sn.GetString() : null;

                    if (types.Contains("street_number")) streetNumber = longName;
                    else if (types.Contains("route")) route = longName;
                    else if (types.Contains("locality")) city ??= longName;
                    else if (types.Contains("administrative_area_level_2")) city ??= longName;
                    else if (types.Contains("administrative_area_level_1")) state = shortName ?? longName;
                    else if (types.Contains("country")) country = shortName; // ISO 3166-1 alpha-2
                    else if (types.Contains("postal_code")) postalCode = longName;
                }
            }

            var address = string.Join(" ", new[] { route, streetNumber }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(address) && first.TryGetProperty("formatted_address", out var formatted))
                address = formatted.GetString();

            return new ReverseGeocodeResult(true, address, city, state, country, postalCode, null);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "No se pudo parsear la respuesta de Google Geocoding API");
            return new ReverseGeocodeResult(false, null, null, null, null, null, "Respuesta de geocodificación inválida.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Error de red al consultar Google Geocoding API");
            return new ReverseGeocodeResult(false, null, null, null, null, null, "No se pudo contactar el servicio de geocodificación.");
        }
    }
}
