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
}