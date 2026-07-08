namespace IntegracionNiubizDemo.Infrastructure.Geocoding;

/// <summary>
/// Configuración para la Google Maps Geocoding API, usada para resolver una dirección
/// postal a partir de coordenadas (reverse geocoding) cuando el usuario usa el botón
/// "Usar mi ubicación" en el checkout. La llamada se hace siempre desde el servidor
/// para no exponer la API key en el navegador.
/// </summary>
public class GoogleMapsOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string GeocodeBaseUrl { get; set; } = "https://maps.googleapis.com/maps/api/geocode/json";
    public string Language { get; set; } = "es";
}
