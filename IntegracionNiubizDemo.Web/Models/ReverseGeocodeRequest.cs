using System.ComponentModel.DataAnnotations;

namespace IntegracionNiubizDemo.Web.Models;

/// <summary>
/// Cuerpo del POST AJAX enviado por el botón "Usar mi ubicación" del checkout,
/// con las coordenadas obtenidas desde el navegador (Geolocation API).
/// </summary>
public class ReverseGeocodeRequest
{
    [Range(-90, 90, ErrorMessage = "Latitud inválida")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitud inválida")]
    public double Longitude { get; set; }
}
