namespace IntegracionNiubizDemo.Domain.Entities;

public enum OrderStatus { Pending = 0, Paid = 1, Rejected = 2, Error = 3 }

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PurchaseNumber { get; set; } = default!; // único por comercio
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? CustomerEmail { get; set; }

    // Datos de facturación/ubicación del comprador, requeridos por Niubiz (dataMap)
    // para el motor antifraude de la sesión. Se capturan en el formulario de checkout
    // (manual, autocomplete de navegador, o geolocalización + reverse geocoding).
    public string? CustomerAddress { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerState { get; set; }        // Departamento/región (código corto, ej. "LIM")
    public string? CustomerCountry { get; set; } = "PE"; // ISO 3166-1 alpha-2
    public string? CustomerPostalCode { get; set; }
    public string? CustomerPhone { get; set; }
}