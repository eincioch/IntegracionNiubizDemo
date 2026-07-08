namespace IntegracionNiubizDemo.Infrastructure.Niubiz;

public class NiubizOptions
{
    public string Environment { get; set; } = "qa";
    public string MerchantId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Currency { get; set; } = "PEN";
    public string BaseUrl { get; set; } = default!;
    public string SecurityEndpoint { get; set; } = "/api.security/v1/security";
    public string SessionEndpoint { get; set; } = "/api.ecommerce/v2/ecommerce/token/session/{merchantId}";
    public string AuthorizationEndpoint { get; set; } = "/api.authorization/v3/authorization/ecommerce/{merchantId}";
    public string StaticJsUrl { get; set; } = default!;

    /// <summary>
    /// URL del comercio requerida por Niubiz en dataMap.urlAddress del API de Autorizaci�n e-commerce.
    /// </summary>
    public string MerchantUrl { get; set; } = "http://localhost:5110";

    // Ubicación del comercio (no del cliente), requerida por Niubiz en dataMap.serviceLocation*
    // del API de Autorización e-commerce.
    public string MerchantCityName { get; set; } = "Lima";
    public string MerchantCountrySubdivisionCode { get; set; } = "LIM";
    public string MerchantCountryCode { get; set; } = "PE";
    public string MerchantPostalCode { get; set; } = "15001";
}