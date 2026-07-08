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

/// <summary>
/// Dirección/ubicación del comprador capturada en el formulario de checkout,
/// usada para poblar el `dataMap` antifraude de la sesión Niubiz.
/// </summary>
public record CustomerAddressInput(
    string Address,
    string City,
    string State,
    string Country,
    string PostalCode,
    string? Phone);

/// <summary>
/// Resultado de geocodificación inversa (lat/lng -> dirección) usado para
/// autocompletar el formulario cuando el usuario usa el botón "Usar mi ubicación".
/// </summary>
public record ReverseGeocodeResult(
    bool Success,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Error);
