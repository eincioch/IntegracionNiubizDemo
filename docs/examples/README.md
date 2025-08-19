# 💳 Ejemplos de Integración con Niubiz

## Casos de Uso Completos

Esta sección proporciona ejemplos prácticos y casos de uso reales para integrar Niubiz en diferentes escenarios.

## 🛒 Ejemplo 1: E-commerce Básico

### Implementación de un Catálogo Simple

```csharp
[ApiController]
[Route("api/[controller]")]
public class EcommerceController : ControllerBase
{
    private readonly ICheckoutService _checkout;
    private readonly IProductService _products;
    private readonly ILogger<EcommerceController> _logger;

    public EcommerceController(
        ICheckoutService checkout, 
        IProductService products,
        ILogger<EcommerceController> logger)
    {
        _checkout = checkout;
        _products = products;
        _logger = logger;
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _products.GetProductsAsync();
        return Ok(products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Currency = "PEN",
            ImageUrl = $"/images/products/{p.Id}.jpg"
        }));
    }

    [HttpPost("checkout/init")]
    public async Task<ActionResult<CheckoutResponse>> InitCheckout(
        [FromBody] InitCheckoutRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando checkout para producto {ProductId}", 
                request.ProductId);

            var result = await _checkout.InitAsync(
                request.ProductId, 
                request.CustomerEmail);

            return Ok(new CheckoutResponse
            {
                Success = true,
                SessionKey = result.SessionKey,
                MerchantId = result.MerchantId,
                PurchaseNumber = result.PurchaseNumber,
                Amount = result.Amount,
                Currency = result.Currency,
                SdkUrl = result.StaticJsUrl
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "Producto no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar checkout");
            return StatusCode(500, new { Error = "Error interno del servidor" });
        }
    }

    [HttpPost("checkout/confirm")]
    public async Task<ActionResult<PaymentResponse>> ConfirmPayment(
        [FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            var result = await _checkout.ConfirmAsync(
                request.PurchaseNumber, 
                request.TransactionToken);

            if (result.Success)
            {
                // Enviar email de confirmación, actualizar inventario, etc.
                await SendConfirmationEmailAsync(request.PurchaseNumber);
                
                return Ok(new PaymentResponse
                {
                    Success = true,
                    AuthorizationCode = result.AuthorizationCode,
                    MaskedCard = result.MaskedCard,
                    Message = "Pago procesado exitosamente"
                });
            }
            else
            {
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = result.Message
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar pago {PurchaseNumber}", 
                request.PurchaseNumber);
            return StatusCode(500, new { Error = "Error al procesar el pago" });
        }
    }

    private async Task SendConfirmationEmailAsync(string purchaseNumber)
    {
        // Implementar envío de email
        _logger.LogInformation("Email de confirmación enviado para {PurchaseNumber}", 
            purchaseNumber);
    }
}
```

### DTOs para el API

```csharp
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "PEN";
    public string? ImageUrl { get; set; }
}

public class InitCheckoutRequest
{
    public Guid ProductId { get; set; }
    public string? CustomerEmail { get; set; }
}

public class CheckoutResponse
{
    public bool Success { get; set; }
    public string SessionKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string PurchaseNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";
    public string SdkUrl { get; set; } = string.Empty;
}

public class ConfirmPaymentRequest
{
    public string PurchaseNumber { get; set; } = string.Empty;
    public string TransactionToken { get; set; } = string.Empty;
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? MaskedCard { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

### Frontend JavaScript

```html
<!DOCTYPE html>
<html>
<head>
    <title>Tienda Online</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body>
    <div class="container mt-4">
        <h2>Productos Disponibles</h2>
        <div id="products-container" class="row">
            <!-- Productos se cargan dinámicamente -->
        </div>
    </div>

    <!-- Modal de Pago -->
    <div class="modal fade" id="paymentModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Procesar Pago</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div id="payment-form">
                        <!-- El SDK de Niubiz se carga aquí -->
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        let currentCheckout = null;

        // Cargar productos al inicio
        document.addEventListener('DOMContentLoaded', loadProducts);

        async function loadProducts() {
            try {
                const response = await fetch('/api/ecommerce/products');
                const products = await response.json();
                
                const container = document.getElementById('products-container');
                container.innerHTML = products.map(product => `
                    <div class="col-md-4 mb-3">
                        <div class="card">
                            <img src="${product.imageUrl || '/images/no-image.jpg'}" 
                                 class="card-img-top" alt="${product.name}" style="height: 200px; object-fit: cover;">
                            <div class="card-body">
                                <h5 class="card-title">${product.name}</h5>
                                <p class="card-text">S/ ${product.price.toFixed(2)}</p>
                                <button class="btn btn-primary" onclick="buyProduct('${product.id}', '${product.name}', ${product.price})">
                                    Comprar
                                </button>
                            </div>
                        </div>
                    </div>
                `).join('');
            } catch (error) {
                console.error('Error cargando productos:', error);
                alert('Error al cargar productos');
            }
        }

        async function buyProduct(productId, productName, price) {
            const email = prompt(`Ingresa tu email para comprar "${productName}"`);
            if (!email) return;

            try {
                // Mostrar loading
                const modal = new bootstrap.Modal(document.getElementById('paymentModal'));
                document.getElementById('payment-form').innerHTML = '<div class="text-center">Cargando...</div>';
                modal.show();

                // Inicializar checkout
                const response = await fetch('/api/ecommerce/checkout/init', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        productId: productId,
                        customerEmail: email
                    })
                });

                if (!response.ok) {
                    throw new Error('Error al inicializar pago');
                }

                const checkout = await response.json();
                currentCheckout = checkout;

                // Cargar SDK de Niubiz
                loadNiubizSDK(checkout);

            } catch (error) {
                console.error('Error en checkout:', error);
                alert('Error al inicializar el pago');
            }
        }

        function loadNiubizSDK(checkout) {
            const form = document.getElementById('payment-form');
            form.innerHTML = `
                <h6>Monto: S/ ${checkout.amount.toFixed(2)}</h6>
                <form id="niubiz-form" action="/api/ecommerce/checkout/confirm" method="post">
                    <input type="hidden" name="purchaseNumber" value="${checkout.purchaseNumber}">
                    <input type="hidden" id="transactionToken" name="transactionToken">
                </form>
            `;

            // Cargar script de Niubiz
            const script = document.createElement('script');
            script.src = checkout.sdkUrl;
            script.setAttribute('data-sessiontoken', checkout.sessionKey);
            script.setAttribute('data-channel', 'web');
            script.setAttribute('data-merchantid', checkout.merchantId);
            script.setAttribute('data-purchasenumber', checkout.purchaseNumber);
            script.setAttribute('data-amount', checkout.amount.toFixed(2));
            script.setAttribute('data-currency', checkout.currency);
            script.setAttribute('data-expirationminutes', '20');
            script.setAttribute('data-timeouturl', 'about:blank');
            script.setAttribute('data-merchantlogo', '');
            script.setAttribute('data-formbuttoncolor', '#007bff');
            script.setAttribute('data-complete', 'onPaymentComplete');

            document.getElementById('niubiz-form').appendChild(script);
        }

        // Callback cuando Niubiz completa el pago
        async function onPaymentComplete(token) {
            try {
                const response = await fetch('/api/ecommerce/checkout/confirm', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        purchaseNumber: currentCheckout.purchaseNumber,
                        transactionToken: token
                    })
                });

                const result = await response.json();
                
                if (result.success) {
                    alert(`¡Pago exitoso!\\nCódigo de autorización: ${result.authorizationCode}\\nTarjeta: ${result.maskedCard}`);
                } else {
                    alert(`Error en el pago: ${result.message}`);
                }

                // Cerrar modal
                bootstrap.Modal.getInstance(document.getElementById('paymentModal')).hide();

            } catch (error) {
                console.error('Error confirmando pago:', error);
                alert('Error al confirmar el pago');
            }
        }
    </script>
</body>
</html>
```

## 🏪 Ejemplo 2: Marketplace Multi-Vendedor

### Modelo de Datos Extendido

```csharp
public class Vendor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; } = 0.05m; // 5% comisión
    public string MerchantId { get; set; } = string.Empty; // Cada vendedor puede tener su propio Merchant ID
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Order
{
    public Guid Id { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal VendorAmount { get; set; }
    public string Currency { get; set; } = "PEN";
    public string? CustomerEmail { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
```

### Servicio de Marketplace

```csharp
public interface IMarketplaceService
{
    Task<CheckoutInitResult> InitCheckoutAsync(Guid productId, string customerEmail, CancellationToken ct = default);
    Task<ConfirmResult> ConfirmPaymentAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default);
    Task<decimal> CalculateCommissionAsync(decimal amount, Guid vendorId, CancellationToken ct = default);
    Task ProcessVendorPayoutAsync(Guid orderId, CancellationToken ct = default);
}

public class MarketplaceService : IMarketplaceService
{
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly IVendorRepository _vendors;
    private readonly INiubizGateway _niubiz;
    private readonly ILogger<MarketplaceService> _logger;

    public MarketplaceService(
        IProductRepository products,
        IOrderRepository orders,
        IVendorRepository vendors,
        INiubizGateway niubiz,
        ILogger<MarketplaceService> logger)
    {
        _products = products;
        _orders = orders;
        _vendors = vendors;
        _niubiz = niubiz;
        _logger = logger;
    }

    public async Task<CheckoutInitResult> InitCheckoutAsync(Guid productId, string customerEmail, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
            ?? throw new KeyNotFoundException("Producto no encontrado");

        var vendor = await _vendors.GetByIdAsync(product.VendorId, ct)
            ?? throw new KeyNotFoundException("Vendedor no encontrado");

        if (product.Stock <= 0)
            throw new InvalidOperationException("Producto sin stock");

        var commissionAmount = await CalculateCommissionAsync(product.Price, vendor.Id, ct);
        var vendorAmount = product.Price - commissionAmount;

        var purchaseNumber = GeneratePurchaseNumber();
        var order = new Order
        {
            PurchaseNumber = purchaseNumber,
            Amount = product.Price,
            CommissionAmount = commissionAmount,
            VendorAmount = vendorAmount,
            Currency = "PEN",
            CustomerEmail = customerEmail,
            VendorId = vendor.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _orders.AddAsync(order, ct);

        // Usar el MerchantId del marketplace principal o del vendedor según configuración
        var securityToken = await _niubiz.GetSecurityTokenAsync(ct);
        var sessionKey = await _niubiz.CreateSessionAsync(
            securityToken, 
            order.Amount, 
            order.PurchaseNumber, 
            order.Currency, 
            ct);

        _logger.LogInformation("Checkout iniciado para marketplace - Producto: {ProductId}, Vendedor: {VendorId}, Comisión: {Commission}",
            productId, vendor.Id, commissionAmount);

        return new CheckoutInitResult(
            "MARKETPLACE_MERCHANT_ID", // Usar el merchant ID del marketplace
            sessionKey,
            order.PurchaseNumber,
            order.Amount,
            order.Currency,
            "https://static-content-qas.vnforapps.com/v2/js/checkout.js?qa=true");
    }

    public async Task<ConfirmResult> ConfirmPaymentAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default)
    {
        var order = await _orders.GetByPurchaseNumberAsync(purchaseNumber, ct)
            ?? throw new KeyNotFoundException("Orden no encontrada");

        var securityToken = await _niubiz.GetSecurityTokenAsync(ct);
        var authResult = await _niubiz.AuthorizeAsync(
            securityToken, 
            transactionToken, 
            order.Amount, 
            order.Currency, 
            order.PurchaseNumber, 
            ct);

        if (authResult.Approved)
        {
            // Procesar pago del vendedor
            await ProcessVendorPayoutAsync(order.Id, ct);
            
            // Actualizar stock
            var product = await _products.GetByIdAsync(order.Id, ct); // Necesitamos ProductId en Order
            if (product != null)
            {
                product.Stock--;
                await _products.UpdateAsync(product, ct);
            }

            _logger.LogInformation("Pago confirmado en marketplace - Orden: {OrderId}, Vendedor recibe: {VendorAmount}",
                order.Id, order.VendorAmount);
        }

        return new ConfirmResult(
            authResult.Approved,
            order.PurchaseNumber,
            authResult.AuthorizationCode,
            authResult.Approved ? "Pago procesado exitosamente" : "Pago rechazado",
            authResult.MaskedCard,
            authResult.RawJson);
    }

    public async Task<decimal> CalculateCommissionAsync(decimal amount, Guid vendorId, CancellationToken ct = default)
    {
        var vendor = await _vendors.GetByIdAsync(vendorId, ct);
        var commissionRate = vendor?.CommissionRate ?? 0.05m; // 5% por defecto
        return Math.Round(amount * commissionRate, 2);
    }

    public async Task ProcessVendorPayoutAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order == null) return;

        // Aquí implementarías la lógica para transferir dinero al vendedor
        // Esto podría ser a través de transferencias bancarias, billeteras digitales, etc.
        
        _logger.LogInformation("Procesando pago a vendedor {VendorId} por monto {Amount} de la orden {OrderId}",
            order.VendorId, order.VendorAmount, order.Id);

        // Simular procesamiento de pago al vendedor
        await Task.Delay(100, ct);
    }

    private static string GeneratePurchaseNumber()
        => DateTime.UtcNow.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);
}
```

## 🎯 Ejemplo 3: Suscripciones y Pagos Recurrentes

### Modelo para Suscripciones

```csharp
public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int BillingCycleDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}

public class Subscription
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionPlan Plan { get; set; } = null!;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CardToken { get; set; } // Token de tarjeta para pagos recurrentes
    public DateTime StartDate { get; set; }
    public DateTime NextBillingDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal CurrentPrice { get; set; }
}

public enum SubscriptionStatus
{
    Active,
    Paused,
    Cancelled,
    PendingPayment,
    Expired
}
```

### Servicio de Suscripciones

```csharp
public interface ISubscriptionService
{
    Task<CheckoutInitResult> InitSubscriptionAsync(Guid planId, string customerEmail, CancellationToken ct = default);
    Task<ConfirmResult> ConfirmSubscriptionAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default);
    Task ProcessRecurringPaymentsAsync(CancellationToken ct = default);
    Task<bool> CancelSubscriptionAsync(Guid subscriptionId, CancellationToken ct = default);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IPlanRepository _plans;
    private readonly INiubizGateway _niubiz;
    private readonly ILogger<SubscriptionService> _logger;

    public async Task<CheckoutInitResult> InitSubscriptionAsync(Guid planId, string customerEmail, CancellationToken ct = default)
    {
        var plan = await _plans.GetByIdAsync(planId, ct)
            ?? throw new KeyNotFoundException("Plan no encontrado");

        if (!plan.IsActive)
            throw new InvalidOperationException("Plan no disponible");

        var purchaseNumber = GeneratePurchaseNumber();
        
        // Crear suscripción en estado pendiente
        var subscription = new Subscription
        {
            PlanId = planId,
            CustomerEmail = customerEmail,
            StartDate = DateTime.UtcNow,
            NextBillingDate = DateTime.UtcNow.AddDays(plan.BillingCycleDays),
            Status = SubscriptionStatus.PendingPayment,
            CurrentPrice = plan.MonthlyPrice
        };

        await _subscriptions.AddAsync(subscription, ct);

        var securityToken = await _niubiz.GetSecurityTokenAsync(ct);
        var sessionKey = await _niubiz.CreateSessionAsync(
            securityToken,
            plan.MonthlyPrice,
            purchaseNumber,
            "PEN",
            ct);

        return new CheckoutInitResult(
            "YOUR_MERCHANT_ID",
            sessionKey,
            purchaseNumber,
            plan.MonthlyPrice,
            "PEN",
            "https://static-content-qas.vnforapps.com/v2/js/checkout.js?qa=true");
    }

    public async Task ProcessRecurringPaymentsAsync(CancellationToken ct = default)
    {
        var dueSubscriptions = await _subscriptions.GetDueForBillingAsync(DateTime.UtcNow, ct);

        foreach (var subscription in dueSubscriptions)
        {
            try
            {
                await ProcessRecurringPaymentAsync(subscription, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando pago recurrente para suscripción {SubscriptionId}",
                    subscription.Id);
                
                subscription.Status = SubscriptionStatus.PendingPayment;
                await _subscriptions.UpdateAsync(subscription, ct);
            }
        }
    }

    private async Task ProcessRecurringPaymentAsync(Subscription subscription, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(subscription.CardToken))
        {
            _logger.LogWarning("Suscripción {SubscriptionId} sin token de tarjeta", subscription.Id);
            return;
        }

        var purchaseNumber = GeneratePurchaseNumber();
        var securityToken = await _niubiz.GetSecurityTokenAsync(ct);

        // Para pagos recurrentes, necesitarías usar el token guardado
        // Nota: Este es un ejemplo simplificado - Niubiz tiene APIs específicas para recurrencia
        var authResult = await _niubiz.AuthorizeAsync(
            securityToken,
            subscription.CardToken,
            subscription.CurrentPrice,
            "PEN",
            purchaseNumber,
            ct);

        if (authResult.Approved)
        {
            subscription.NextBillingDate = subscription.NextBillingDate.AddDays(subscription.Plan.BillingCycleDays);
            subscription.Status = SubscriptionStatus.Active;
            
            _logger.LogInformation("Pago recurrente exitoso para suscripción {SubscriptionId}", 
                subscription.Id);
        }
        else
        {
            subscription.Status = SubscriptionStatus.PendingPayment;
            
            _logger.LogWarning("Pago recurrente falló para suscripción {SubscriptionId}: {Reason}",
                subscription.Id, authResult.RawJson);
        }

        await _subscriptions.UpdateAsync(subscription, ct);
    }

    private static string GeneratePurchaseNumber()
        => $"SUB_{DateTime.UtcNow:yyMMddHHmmss}";
}
```

## 🔄 Ejemplo 4: Webhook Handler

### Controlador de Webhooks

```csharp
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost("niubiz")]
    public async Task<IActionResult> NiubizWebhook([FromBody] NiubizWebhookDto webhook)
    {
        try
        {
            // Validar firma del webhook
            if (!await _webhookService.ValidateSignatureAsync(Request, webhook))
            {
                _logger.LogWarning("Webhook con firma inválida recibido desde {IP}", 
                    Request.HttpContext.Connection.RemoteIpAddress);
                return Unauthorized();
            }

            // Procesar evento
            await _webhookService.ProcessWebhookAsync(webhook);
            
            return Ok(new { status = "processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook de Niubiz");
            return StatusCode(500);
        }
    }

    [HttpPost("niubiz/test")]
    [ApiExplorerSettings(IgnoreApi = true)] // Solo para testing
    public IActionResult TestWebhook()
    {
        return Ok(new { status = "test_endpoint_active" });
    }
}

public class NiubizWebhookDto
{
    public string EventType { get; set; } = string.Empty;
    public string PurchaseNumber { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Signature { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public interface IWebhookService
{
    Task<bool> ValidateSignatureAsync(HttpRequest request, NiubizWebhookDto webhook);
    Task ProcessWebhookAsync(NiubizWebhookDto webhook);
}

public class WebhookService : IWebhookService
{
    private readonly IOrderRepository _orders;
    private readonly IPaymentRepository _payments;
    private readonly IConfiguration _config;
    private readonly ILogger<WebhookService> _logger;

    public async Task<bool> ValidateSignatureAsync(HttpRequest request, NiubizWebhookDto webhook)
    {
        var secretKey = _config["Niubiz:WebhookSecret"];
        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogWarning("Webhook secret no configurado");
            return false;
        }

        // Implementar validación HMAC
        var bodyJson = JsonSerializer.Serialize(webhook);
        var expectedSignature = ComputeHmacSha256(bodyJson, secretKey);
        
        return webhook.Signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
    }

    public async Task ProcessWebhookAsync(NiubizWebhookDto webhook)
    {
        _logger.LogInformation("Procesando webhook {EventType} para compra {PurchaseNumber}",
            webhook.EventType, webhook.PurchaseNumber);

        switch (webhook.EventType.ToLowerInvariant())
        {
            case "payment.approved":
                await HandlePaymentApprovedAsync(webhook);
                break;
            case "payment.declined":
                await HandlePaymentDeclinedAsync(webhook);
                break;
            case "payment.refunded":
                await HandlePaymentRefundedAsync(webhook);
                break;
            case "payment.chargeback":
                await HandleChargebackAsync(webhook);
                break;
            default:
                _logger.LogWarning("Tipo de evento no manejado: {EventType}", webhook.EventType);
                break;
        }
    }

    private async Task HandlePaymentApprovedAsync(NiubizWebhookDto webhook)
    {
        var payment = await _payments.GetByPurchaseNumberAsync(webhook.PurchaseNumber);
        if (payment != null)
        {
            payment.Status = "APPROVED";
            payment.ProcessedAt = webhook.Timestamp;
            await _payments.UpdateAsync(payment);
        }
    }

    private async Task HandlePaymentDeclinedAsync(NiubizWebhookDto webhook)
    {
        var payment = await _payments.GetByPurchaseNumberAsync(webhook.PurchaseNumber);
        if (payment != null)
        {
            payment.Status = "DECLINED";
            payment.ProcessedAt = webhook.Timestamp;
            await _payments.UpdateAsync(payment);
        }
    }

    private async Task HandlePaymentRefundedAsync(NiubizWebhookDto webhook)
    {
        // Lógica para manejar reembolsos
        _logger.LogInformation("Reembolso procesado para {PurchaseNumber}", webhook.PurchaseNumber);
    }

    private async Task HandleChargebackAsync(NiubizWebhookDto webhook)
    {
        // Lógica para manejar contracargos
        _logger.LogWarning("Contracargo recibido para {PurchaseNumber}", webhook.PurchaseNumber);
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
```

## 🧪 Casos de Prueba

### Unit Tests con xUnit

```csharp
public class CheckoutServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<INiubizGateway> _mockNiubizGateway;
    private readonly CheckoutService _service;

    public CheckoutServiceTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
        _mockNiubizGateway = new Mock<INiubizGateway>();
        // ... setup otros mocks
        
        _service = new CheckoutService(
            _mockProductRepo.Object,
            _mockNiubizGateway.Object
            // ... otros parámetros
        );
    }

    [Fact]
    public async Task InitAsync_WithValidProduct_ShouldReturnCheckoutResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", Price = 100m };
        
        _mockProductRepo.Setup(x => x.GetByIdAsync(productId, default))
            .ReturnsAsync(product);
        _mockNiubizGateway.Setup(x => x.GetSecurityTokenAsync(default))
            .ReturnsAsync("test_token");
        _mockNiubizGateway.Setup(x => x.CreateSessionAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync("test_session");

        // Act
        var result = await _service.InitAsync(productId, "test@email.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100m, result.Amount);
        Assert.Equal("test_session", result.SessionKey);
    }

    [Fact]
    public async Task InitAsync_WithInvalidProduct_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductRepo.Setup(x => x.GetByIdAsync(productId, default))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _service.InitAsync(productId, "test@email.com"));
    }
}
```

### Integration Tests

```csharp
public class NiubizIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public NiubizIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CheckoutFlow_EndToEnd_ShouldProcessSuccessfully()
    {
        // Arrange - Crear producto de prueba
        var product = await CreateTestProductAsync();

        // Act 1 - Inicializar checkout
        var initResponse = await _client.GetAsync($"/checkout/pay/{product.Id}?email=test@email.com");
        initResponse.EnsureSuccessStatusCode();

        // Extraer datos del checkout de la respuesta HTML
        var html = await initResponse.Content.ReadAsStringAsync();
        var purchaseNumber = ExtractPurchaseNumberFromHtml(html);
        
        // Act 2 - Simular confirmación (en pruebas reales usarías un token de prueba)
        var confirmData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("purchaseNumber", purchaseNumber),
            new KeyValuePair<string, string>("transactionToken", "TEST_TOKEN")
        });

        var confirmResponse = await _client.PostAsync("/checkout/confirm", confirmData);
        
        // Assert
        confirmResponse.EnsureSuccessStatusCode();
        var confirmHtml = await confirmResponse.Content.ReadAsStringAsync();
        Assert.Contains("procesado", confirmHtml); // O el mensaje de éxito que uses
    }

    private async Task<Product> CreateTestProductAsync()
    {
        // Crear producto de prueba en la base de datos
        // Implementar según tu setup de testing
        return new Product { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m };
    }

    private string ExtractPurchaseNumberFromHtml(string html)
    {
        // Extraer purchaseNumber del HTML usando regex o parser HTML
        var match = Regex.Match(html, @"data-purchasenumber=""([^""]+)""");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
```

## 📊 Monitoreo y Métricas

### Configuración de Application Insights

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Métricas personalizadas
builder.Services.AddSingleton<TelemetryClient>();

public class PaymentMetrics
{
    private readonly TelemetryClient _telemetry;

    public PaymentMetrics(TelemetryClient telemetry)
    {
        _telemetry = telemetry;
    }

    public void TrackPaymentInitiated(decimal amount, string currency)
    {
        _telemetry.TrackEvent("PaymentInitiated", new Dictionary<string, string>
        {
            ["Currency"] = currency
        }, new Dictionary<string, double>
        {
            ["Amount"] = (double)amount
        });
    }

    public void TrackPaymentCompleted(bool success, string reason = "")
    {
        _telemetry.TrackEvent("PaymentCompleted", new Dictionary<string, string>
        {
            ["Success"] = success.ToString(),
            ["Reason"] = reason
        });
    }
}
```

Estos ejemplos proporcionan casos de uso reales y prácticos que los desarrolladores pueden usar como base para sus propias implementaciones con Niubiz.