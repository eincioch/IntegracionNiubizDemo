using IntegracionNiubizDemo.Application.Dtos;

namespace IntegracionNiubizDemo.Application.Abstractions;

/// <summary>
/// Abstracción para resolver una dirección postal a partir de coordenadas de geolocalización
/// del navegador (reverse geocoding), usada por el botón "Usar mi ubicación" del checkout.
/// </summary>
public interface IGeocodingService
{
    Task<ReverseGeocodeResult> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken ct = default);
}
