namespace IntegracionNiubizDemo.Application.Dtos;

public record CheckoutInitResult(
    string MerchantId,
    string SessionKey,
    string PurchaseNumber,
    decimal Amount,
    string Currency,
    string StaticJsUrl);

public record AuthorizationResult(
    bool Approved,
    string? AuthorizationCode,
    string? MaskedCard,
    string RawJson);

public record ConfirmResult(
    bool Success,
    string PurchaseNumber,
    string? AuthorizationCode,
    string Message,
    string? MaskedCard,
    string RawJson);