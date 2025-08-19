# 🔌 Documentación de APIs

## Resumen de APIs Disponibles

Esta documentación detalla todas las APIs y endpoints disponibles en el sistema de integración con Niubiz.

## 📡 APIs Web (Controllers)

### ProductsController

#### `GET /` - Listar Productos
Obtiene la lista de todos los productos disponibles.

**Request:**
```http
GET / HTTP/1.1
Host: localhost:7001
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: text/html

<!-- Vista HTML con lista de productos -->
```

**Modelo de Vista:**
```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

---

### CheckoutController

#### `GET /checkout/pay/{productId}` - Inicializar Pago
Inicia el proceso de pago para un producto específico.

**Parámetros:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `productId` | `Guid` | ✅ | ID único del producto |
| `email` | `string` | ❌ | Email del cliente (query parameter) |

**Request:**
```http
GET /checkout/pay/550e8400-e29b-41d4-a716-446655440000?email=cliente@email.com HTTP/1.1
Host: localhost:7001
```

**Response (Success):**
```http
HTTP/1.1 200 OK
Content-Type: text/html

<!-- Página con formulario de pago y SDK de Niubiz -->
```

**Response (Product Not Found):**
```http
HTTP/1.1 404 Not Found
Content-Type: text/html

<!-- Página de error -->
```

**Modelo de Datos Interno:**
```csharp
public record CheckoutInitResult(
    string MerchantId,       // "123456789"
    string SessionKey,       // "abc123def456..."
    string PurchaseNumber,   // "231215143022"
    decimal Amount,          // 99.99
    string Currency,         // "PEN"
    string StaticJsUrl       // "https://static-content-qas.vnforapps.com/..."
);
```

#### `POST /checkout/confirm` - Confirmar Pago
Confirma y procesa el pago con el token generado por Niubiz.

**Content-Type:** `application/x-www-form-urlencoded`

**Parámetros del Form:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `purchaseNumber` | `string` | ✅ | Número de compra generado |
| `transactionToken` | `string` | ✅ | Token de transacción de Niubiz |
| `tokenId` | `string` | ❌ | Token alternativo (alias) |
| `token` | `string` | ❌ | Token alternativo (alias) |

**Request:**
```http
POST /checkout/confirm HTTP/1.1
Host: localhost:7001
Content-Type: application/x-www-form-urlencoded

purchaseNumber=231215143022&transactionToken=abc123def456ghi789
```

**Response (Success):**
```http
HTTP/1.1 200 OK
Content-Type: text/html

<!-- Página de confirmación exitosa -->
```

**Response (Error):**
```http
HTTP/1.1 200 OK
Content-Type: text/html

<!-- Página de error con detalles -->
```

**Modelo de Respuesta Interno:**
```csharp
public record ConfirmResult(
    bool Success,               // true/false
    string PurchaseNumber,      // "231215143022"
    string? AuthorizationCode,  // "123456" o null
    string Message,             // "Pago procesado exitosamente"
    string? MaskedCard,         // "****-****-****-1234" o null
    string RawJson              // Respuesta completa de Niubiz
);
```

## 🔧 APIs de Servicio (Application Layer)

### ICheckoutService

```csharp
public interface ICheckoutService
{
    Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail, CancellationToken ct = default);
    Task<ConfirmResult> ConfirmAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default);
}
```

#### `InitAsync` - Inicializar Checkout

**Propósito:** Inicia el proceso de checkout creando una sesión de pago con Niubiz.

**Parámetros:**
- `productId` (Guid): ID del producto a comprar
- `customerEmail` (string?, opcional): Email del cliente
- `ct` (CancellationToken): Token de cancelación

**Retorna:** `CheckoutInitResult` con datos necesarios para el SDK de Niubiz

**Excepciones:**
- `KeyNotFoundException`: Producto no encontrado
- `NiubizApiException`: Error en API de Niubiz
- `InvalidOperationException`: Configuración inválida

**Ejemplo de Uso:**
```csharp
try
{
    var result = await _checkoutService.InitAsync(
        productId: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
        customerEmail: "cliente@email.com"
    );
    
    // Usar result.SessionKey, result.MerchantId, etc.
}
catch (KeyNotFoundException)
{
    // Manejar producto no encontrado
}
```

#### `ConfirmAsync` - Confirmar Pago

**Propósito:** Autoriza la transacción usando el token generado por Niubiz.

**Parámetros:**
- `purchaseNumber` (string): Número de compra único
- `transactionToken` (string): Token de transacción de Niubiz
- `ct` (CancellationToken): Token de cancelación

**Retorna:** `ConfirmResult` con el resultado del procesamiento

**Ejemplo de Uso:**
```csharp
var result = await _checkoutService.ConfirmAsync(
    purchaseNumber: "231215143022",
    transactionToken: "abc123def456ghi789"
);

if (result.Success)
{
    // Pago exitoso
    var authCode = result.AuthorizationCode;
    var maskedCard = result.MaskedCard;
}
else
{
    // Pago fallido
    var errorMessage = result.Message;
}
```

---

### INiubizGateway

```csharp
public interface INiubizGateway
{
    Task<string> GetSecurityTokenAsync(CancellationToken ct = default);
    Task<string> CreateSessionAsync(string securityToken, decimal amount, string purchaseNumber, string currency, CancellationToken ct = default);
    Task<AuthorizationResult> AuthorizeAsync(string securityToken, string transactionToken, decimal amount, string currency, string purchaseNumber, CancellationToken ct = default);
}
```

#### `GetSecurityTokenAsync` - Obtener Token de Seguridad

**Propósito:** Autentica con Niubiz y obtiene un token de seguridad.

**Endpoint Niubiz:** `POST /api.security/v1/security`

**Request a Niubiz:**
```json
{
  "merchantId": "123456789",
  "username": "tu_usuario",
  "password": "tu_password"
}
```

**Response de Niubiz:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Retorna:** Token JWT como string

#### `CreateSessionAsync` - Crear Sesión de Pago

**Propósito:** Crea una sesión de pago en Niubiz.

**Parámetros:**
- `securityToken` (string): Token obtenido de GetSecurityTokenAsync
- `amount` (decimal): Monto en soles (formato: 99.99)
- `purchaseNumber` (string): Número único de compra
- `currency` (string): Código de moneda ("PEN")

**Endpoint Niubiz:** `POST /api.ecommerce/v2/ecommerce/token/session/{merchantId}`

**Request a Niubiz:**
```json
{
  "amount": 99.99,
  "antifraud": {
    "clientIp": "192.168.1.1",
    "merchantDefineData": {
      "MDD4": "cliente@email.com",
      "MDD21": "0",
      "MDD32": "20231215143022",
      "MDD75": "Registrado",
      "MDD77": "999"
    }
  },
  "channel": "web"
}
```

**Response de Niubiz:**
```json
{
  "sessionKey": "ABC123DEF456GHI789...",
  "expirationTime": 1200000
}
```

**Retorna:** Session key como string

#### `AuthorizeAsync` - Autorizar Transacción

**Propósito:** Autoriza el pago usando el transaction token.

**Parámetros:**
- `securityToken` (string): Token de seguridad
- `transactionToken` (string): Token de transacción del SDK
- `amount` (decimal): Monto a autorizar
- `currency` (string): Código de moneda
- `purchaseNumber` (string): Número de compra

**Endpoint Niubiz:** `POST /api.authorization/v3/authorization/ecommerce/{merchantId}`

**Request a Niubiz:**
```json
{
  "antifraud": null,
  "captureType": "manual",
  "channel": "web",
  "countable": true,
  "order": {
    "amount": 99.99,
    "currency": "PEN",
    "purchaseNumber": "231215143022",
    "tokenId": "abc123def456ghi789"
  },
  "recurrence": {
    "amount": 99.99,
    "beneficiaryId": "0",
    "frequency": "FALSE",
    "maxAmount": 99.99,
    "type": ""
  }
}
```

**Response de Niubiz (Exitosa):**
```json
{
  "order": {
    "actionCode": "000",
    "authorizationCode": "123456",
    "purchaseNumber": "231215143022",
    "tokenId": "abc123def456ghi789"
  },
  "dataMap": {
    "CARD": "****-****-****-1234",
    "BRAND": "visa"
  }
}
```

**Response de Niubiz (Rechazada):**
```json
{
  "order": {
    "actionCode": "201",
    "purchaseNumber": "231215143022"
  },
  "dataMap": {
    "ACTION_DESCRIPTION": "Tarjeta rechazada"
  }
}
```

**Retorna:** `AuthorizationResult`

```csharp
public record AuthorizationResult(
    bool Approved,              // true si actionCode == "000"
    string? AuthorizationCode,  // Código de autorización del banco
    string? MaskedCard,         // Número de tarjeta enmascarado
    string RawJson              // Respuesta completa de Niubiz
);
```

## 🗄️ APIs de Repositorio (Data Layer)

### IProductRepository

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Product> products, CancellationToken ct = default);
}
```

### IOrderRepository

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByPurchaseNumberAsync(string purchaseNumber, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
```

### IPaymentRepository

```csharp
public interface IPaymentRepository
{
    Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<PaymentTransaction?> GetByPurchaseNumberAsync(string purchaseNumber, CancellationToken ct = default);
    Task AddAsync(PaymentTransaction payment, CancellationToken ct = default);
    Task UpdateAsync(PaymentTransaction payment, CancellationToken ct = default);
}
```

## 📝 Modelos de Datos

### Product
```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### Order
```csharp
public class Order
{
    public Guid Id { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";
    public string? CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### PaymentTransaction
```csharp
public class PaymentTransaction
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string SessionKey { get; set; } = string.Empty;
    public string? TransactionToken { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? MaskedCard { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RawResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Navegación
    public Order Order { get; set; } = null!;
}
```

## 🔐 Códigos de Respuesta de Niubiz

### Action Codes Comunes

| Código | Descripción | Estado |
|--------|-------------|---------|
| `000` | Transacción aprobada | ✅ Exitosa |
| `101` | Tarjeta expirada | ❌ Rechazada |
| `102` | Tarjeta restringida | ❌ Rechazada |
| `104` | Monto no permitido | ❌ Rechazada |
| `106` | PIN incorrecto | ❌ Rechazada |
| `107` | Tarjeta no válida | ❌ Rechazada |
| `201` | Tarjeta rechazada | ❌ Rechazada |
| `202` | Tarjeta robada | ❌ Rechazada |
| `203` | Tarjeta perdida | ❌ Rechazada |
| `291` | Banco no disponible | ❌ Error del banco |
| `999` | Error del sistema | ❌ Error interno |

## 🚨 Manejo de Errores

### Excepciones Personalizadas

```csharp
public class NiubizApiException : Exception
{
    public string? ErrorCode { get; }
    public string? RequestId { get; }
    
    public NiubizApiException(string message, string? errorCode = null, string? requestId = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        RequestId = requestId;
    }
}
```

### Responses de Error Típicos

#### Error de Autenticación (401)
```json
{
  "error": "Unauthorized",
  "message": "Invalid credentials"
}
```

#### Error de Validación (400)
```json
{
  "error": "Bad Request",
  "message": "Invalid amount format"
}
```

#### Error del Servidor (500)
```json
{
  "error": "Internal Server Error",
  "message": "An unexpected error occurred"
}
```

## 📊 Logging y Monitoreo

### Eventos de Log Importantes

```csharp
// Nivel Information
_logger.LogInformation("Payment initialized for product {ProductId} amount {Amount}", 
    productId, amount);

// Nivel Warning  
_logger.LogWarning("Payment failed for purchase {PurchaseNumber} with code {ActionCode}",
    purchaseNumber, actionCode);

// Nivel Error
_logger.LogError(ex, "Niubiz API error for purchase {PurchaseNumber}", purchaseNumber);
```

### Métricas Recomendadas

- **Tasa de éxito de pagos**: Porcentaje de transacciones aprobadas
- **Tiempo de respuesta**: Latencia promedio de las APIs
- **Volumen de transacciones**: Cantidad de pagos por período
- **Errores por tipo**: Distribución de códigos de error

## 🧪 Testing de APIs

### Ejemplos con curl

#### Obtener Security Token
```bash
curl -X POST https://apisandbox.vnforapps.com/api.security/v1/security \
  -H "Content-Type: application/json" \
  -d '{
    "merchantId": "123456789",
    "username": "tu_usuario", 
    "password": "tu_password"
  }'
```

#### Crear Sesión
```bash
curl -X POST https://apisandbox.vnforapps.com/api.ecommerce/v2/ecommerce/token/session/123456789 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_SECURITY_TOKEN" \
  -d '{
    "amount": 99.99,
    "antifraud": {
      "clientIp": "192.168.1.1"
    },
    "channel": "web"
  }'
```

### Postman Collection

Importa esta collection para testing:

```json
{
  "info": {
    "name": "Niubiz Integration Demo",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get Security Token",
      "request": {
        "method": "POST",
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"merchantId\": \"{{merchantId}}\",\n  \"username\": \"{{username}}\",\n  \"password\": \"{{password}}\"\n}"
        },
        "url": "{{baseUrl}}/api.security/v1/security"
      }
    }
  ]
}
```