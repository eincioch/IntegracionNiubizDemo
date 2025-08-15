using System.Globalization;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Application.Dtos;
using IntegracionNiubizDemo.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace IntegracionNiubizDemo.Application.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly IPaymentRepository _payments;
    private readonly INiubizGateway _niubiz;
    private readonly string _currency;
    private readonly string _merchantId;
    private readonly string _staticJsUrl;

    public CheckoutService(
        IProductRepository products,
        IOrderRepository orders,
        IPaymentRepository payments,
        INiubizGateway niubiz,
        IConfiguration config)
    {
        _products = products;
        _orders = orders;
        _payments = payments;
        _niubiz = niubiz;

        _currency = config["Niubiz:Currency"] ?? "PEN";
        _merchantId = config["Niubiz:MerchantId"] ?? throw new InvalidOperationException("Niubiz:MerchantId vacío");
        var env = config["Niubiz:Environment"] ?? "qa";
        _staticJsUrl = config[$"Niubiz:StaticContent:{env}"] ?? throw new InvalidOperationException("Static JS URL vacío");
    }

    public async Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail, CancellationToken ct = default)
    {
        var productList = await _products.GetAllAsync(ct);
        var product = await _products.GetByIdAsync(productId, ct);
        if (product is null) throw new KeyNotFoundException("Producto no encontrado");

        var purchaseNumber = GeneratePurchaseNumber();
        var order = new Order
        {
            PurchaseNumber = purchaseNumber,
            Amount = decimal.Round(product.Price, 2, MidpointRounding.AwayFromZero),
            Currency = _currency,
            CustomerEmail = customerEmail
        };
        await _orders.AddAsync(order, ct);

        var secToken = await _niubiz.GetSecurityTokenAsync(ct);
        var sessionKey = await _niubiz.CreateSessionAsync(secToken, order.Amount, order.PurchaseNumber, order.Currency, ct);

        var txn = new PaymentTransaction
        {
            OrderId = order.Id,
            SessionKey = sessionKey,
            Status = "SESSION_CREATED"
        };
        await _payments.AddAsync(txn, ct);

        return new CheckoutInitResult(_merchantId, sessionKey, order.PurchaseNumber, order.Amount, order.Currency, _staticJsUrl);
    }

    public async Task<ConfirmResult> ConfirmAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default)
    {
        var order = await _orders.GetByPurchaseNumberAsync(purchaseNumber, ct)
            ?? throw new InvalidOperationException("Orden no encontrada");

        //generar security token
        var securityToken = await _niubiz.GetSecurityTokenAsync(ct);


        // Recuperar la transacción creada en InitAsync para obtener el sessionKey
        var txn = await _payments.GetByOrderIdAsync(order.Id, ct);
        if (txn is null || string.IsNullOrWhiteSpace(txn.SessionKey))
        {
            return new ConfirmResult(false, order.PurchaseNumber, null, "SessionKey no encontrado para la orden", null, "{}");
        }

        var auth = await _niubiz.AuthorizeAsync(securityToken, transactionToken, order.Amount, order.Currency, order.PurchaseNumber, ct);

        order.Status = auth.Approved ? OrderStatus.Paid : OrderStatus.Rejected;
        await _orders.UpdateAsync(order, ct);

        // Actualizar/crear transacción con el resultado de la autorización
        txn.TransactionToken = transactionToken;
        txn.AuthorizationCode = auth.AuthorizationCode;
        txn.MaskedCard = auth.MaskedCard;
        txn.Status = auth.Approved ? "AUTHORIZED" : "DECLINED";
        txn.RawResponse = auth.RawJson;
        await _payments.UpdateAsync(txn, ct);

        var msg = auth.Approved ? "Pago aprobado" : "Pago rechazado";
        return new ConfirmResult(auth.Approved, order.PurchaseNumber, auth.AuthorizationCode, msg, auth.MaskedCard, auth.RawJson);
    }

    private static string GeneratePurchaseNumber()
        => DateTime.UtcNow.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture); // único y ordenable
}