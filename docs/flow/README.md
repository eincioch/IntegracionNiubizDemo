# 🔄 Flujo de Pago Niubiz - Documentación Técnica

## Diagrama de Secuencia Completo

```mermaid
sequenceDiagram
    participant Cliente as 👤 Cliente
    participant Browser as 🌐 Navegador
    participant WebApp as 🖥️ Web App
    participant CheckoutSvc as 🧠 CheckoutService
    participant NiubizClient as 🔌 NiubizClient
    participant NiubizAPI as 🏦 Niubiz API
    participant Database as 💾 Base de Datos
    participant NiubizSDK as 📱 SDK Niubiz

    Note over Cliente, NiubizSDK: 🚀 FASE 1: INICIALIZACIÓN DE PAGO

    Cliente->>Browser: Selecciona producto y hace clic en "Comprar"
    Browser->>WebApp: GET /checkout/pay/{productId}?email=cliente@email.com
    
    WebApp->>CheckoutSvc: InitAsync(productId, email)
    Note over CheckoutSvc: Validar producto y crear orden

    CheckoutSvc->>Database: Buscar producto por ID
    Database-->>CheckoutSvc: Datos del producto

    CheckoutSvc->>Database: Crear nueva orden (Order)
    Database-->>CheckoutSvc: Orden creada con PurchaseNumber

    Note over CheckoutSvc, NiubizAPI: 🔐 AUTENTICACIÓN CON NIUBIZ
    
    CheckoutSvc->>NiubizClient: GetSecurityTokenAsync()
    NiubizClient->>NiubizAPI: POST /api.security/v1/security
    Note right of NiubizAPI: Credenciales:<br/>- merchantId<br/>- username<br/>- password
    NiubizAPI-->>NiubizClient: JWT Security Token
    NiubizClient-->>CheckoutSvc: Security Token

    Note over CheckoutSvc, NiubizAPI: 🎫 CREACIÓN DE SESIÓN

    CheckoutSvc->>NiubizClient: CreateSessionAsync(token, amount, purchaseNumber, currency)
    NiubizClient->>NiubizAPI: POST /api.ecommerce/v2/ecommerce/token/session/{merchantId}
    Note right of NiubizAPI: Request Body:<br/>- amount<br/>- antifraud<br/>- channel: "web"
    NiubizAPI-->>NiubizClient: Session Key + expirationTime
    NiubizClient-->>CheckoutSvc: Session Key

    CheckoutSvc->>Database: Crear PaymentTransaction
    Database-->>CheckoutSvc: Transacción guardada

    CheckoutSvc-->>WebApp: CheckoutInitResult
    Note over WebApp: Preparar página de pago con:<br/>- SessionKey<br/>- MerchantId<br/>- PurchaseNumber<br/>- Amount<br/>- StaticJsUrl

    WebApp-->>Browser: HTML + Script SDK Niubiz
    Browser->>NiubizSDK: Cargar SDK con configuración
    NiubizSDK-->>Browser: Formulario de pago renderizado

    Note over Cliente, NiubizSDK: 💳 FASE 2: CAPTURA DE DATOS DE TARJETA

    Cliente->>NiubizSDK: Ingresa datos de tarjeta
    Note over NiubizSDK: Validación local:<br/>- Formato de tarjeta<br/>- CVV<br/>- Fecha expiración

    NiubizSDK->>NiubizAPI: Tokenizar tarjeta (conexión segura)
    NiubizAPI-->>NiubizSDK: Transaction Token
    
    Note over Cliente, NiubizSDK: ✅ FASE 3: CONFIRMACIÓN DE PAGO

    NiubizSDK->>Browser: Enviar formulario con Transaction Token
    Browser->>WebApp: POST /checkout/confirm (purchaseNumber + transactionToken)

    WebApp->>CheckoutSvc: ConfirmAsync(purchaseNumber, transactionToken)
    
    CheckoutSvc->>Database: Buscar orden por purchaseNumber
    Database-->>CheckoutSvc: Datos de la orden

    CheckoutSvc->>Database: Buscar transacción de pago
    Database-->>CheckoutSvc: PaymentTransaction

    Note over CheckoutSvc, NiubizAPI: 🏦 AUTORIZACIÓN DE PAGO

    CheckoutSvc->>NiubizClient: AuthorizeAsync(securityToken, transactionToken, amount, currency, purchaseNumber)
    NiubizClient->>NiubizAPI: POST /api.authorization/v3/authorization/ecommerce/{merchantId}
    
    Note right of NiubizAPI: Request Body:<br/>- order (amount, currency, purchaseNumber, tokenId)<br/>- captureType: "manual"<br/>- channel: "web"<br/>- countable: true<br/>- recurrence settings

    alt Pago Aprobado
        NiubizAPI-->>NiubizClient: ✅ Respuesta exitosa (actionCode: "000")
        Note right of NiubizAPI: Response:<br/>- authorizationCode<br/>- maskedCard<br/>- actionCode: "000"
        
        NiubizClient-->>CheckoutSvc: AuthorizationResult (approved=true)
        CheckoutSvc->>Database: Actualizar PaymentTransaction (SUCCESS)
        Database-->>CheckoutSvc: Transacción actualizada
        CheckoutSvc-->>WebApp: ConfirmResult (success=true)
        WebApp-->>Browser: Página de éxito con código de autorización
        Browser-->>Cliente: ✅ "¡Pago exitoso! Código: ABC123"

    else Pago Rechazado
        NiubizAPI-->>NiubizClient: ❌ Respuesta de rechazo (actionCode: "201" u otro)
        Note right of NiubizAPI: Response:<br/>- actionCode: "201"<br/>- ACTION_DESCRIPTION<br/>- Razón del rechazo
        
        NiubizClient-->>CheckoutSvc: AuthorizationResult (approved=false)
        CheckoutSvc->>Database: Actualizar PaymentTransaction (FAILED)
        Database-->>CheckoutSvc: Transacción actualizada
        CheckoutSvc-->>WebApp: ConfirmResult (success=false)
        WebApp-->>Browser: Página de error con mensaje
        Browser-->>Cliente: ❌ "Pago rechazado: [razón]"

    else Error de Comunicación
        NiubizAPI-->>NiubizClient: ⚠️ Error HTTP/Timeout
        NiubizClient-->>CheckoutSvc: Exception
        CheckoutSvc->>Database: Actualizar PaymentTransaction (ERROR)
        CheckoutSvc-->>WebApp: ConfirmResult (success=false)
        WebApp-->>Browser: Página de error técnico
        Browser-->>Cliente: ⚠️ "Error técnico, intente nuevamente"
    end

    Note over Cliente, Database: 📊 FASE 4: POST-PROCESAMIENTO (OPCIONAL)

    opt Webhook desde Niubiz
        NiubizAPI->>WebApp: POST /api/webhooks/niubiz (confirmación asíncrona)
        WebApp->>Database: Actualizar estado final
    end

    opt Notificaciones
        WebApp->>Cliente: Email de confirmación
        WebApp->>Cliente: SMS (opcional)
    end
```

## Estados de las Entidades

### Ciclo de Vida de Order

```mermaid
stateDiagram-v2
    [*] --> Created
    Created --> Processing: InitAsync()
    Processing --> Completed: Pago exitoso
    Processing --> Failed: Pago rechazado
    Processing --> Error: Error técnico
    Failed --> Processing: Reintentar pago
    Error --> Processing: Reintentar pago
    Completed --> [*]
    Failed --> [*]: Timeout/Abandono
    Error --> [*]: Timeout/Abandono
```

### Estados de PaymentTransaction

```mermaid
stateDiagram-v2
    [*] --> SESSION_CREATED
    SESSION_CREATED --> TOKEN_RECEIVED: SDK retorna token
    TOKEN_RECEIVED --> AUTHORIZING: Enviando a Niubiz
    AUTHORIZING --> APPROVED: actionCode = "000"
    AUTHORIZING --> DECLINED: actionCode != "000"
    AUTHORIZING --> ERROR: Exception/Timeout
    APPROVED --> [*]
    DECLINED --> [*]
    ERROR --> RETRY: Manual retry
    RETRY --> AUTHORIZING
    ERROR --> [*]: Abandono
```

## Flujo de Datos Detallado

### 1. Inicialización (CheckoutService.InitAsync)

```mermaid
flowchart TD
    A[InitAsync] --> B{Producto existe?}
    B -->|No| B1[Throw KeyNotFoundException]
    B -->|Sí| C[Crear Order en DB]
    C --> D[Obtener Security Token]
    D --> E{Token obtenido?}
    E -->|No| E1[Throw NiubizApiException]
    E -->|Sí| F[Crear Session en Niubiz]
    F --> G{Session creada?}
    G -->|No| G1[Throw NiubizApiException]
    G -->|Sí| H[Crear PaymentTransaction en DB]
    H --> I[Retornar CheckoutInitResult]

    style A fill:#e1f5fe
    style I fill:#c8e6c9
    style B1 fill:#ffcdd2
    style E1 fill:#ffcdd2
    style G1 fill:#ffcdd2
```

### 2. Confirmación (CheckoutService.ConfirmAsync)

```mermaid
flowchart TD
    A[ConfirmAsync] --> B{Order existe?}
    B -->|No| B1[Throw KeyNotFoundException]
    B -->|Sí| C{PaymentTransaction existe?}
    C -->|No| C1[Throw InvalidOperationException]
    C -->|Sí| D[Obtener Security Token]
    D --> E[Llamar Niubiz Authorization API]
    E --> F{Respuesta de Niubiz}
    F -->|actionCode = "000"| G[Actualizar como APPROVED]
    F -->|actionCode != "000"| H[Actualizar como DECLINED]
    F -->|Exception| I[Actualizar como ERROR]
    G --> J[Retornar Success=true]
    H --> K[Retornar Success=false]
    I --> L[Retornar Success=false con error]

    style A fill:#e1f5fe
    style J fill:#c8e6c9
    style K fill:#fff3e0
    style L fill:#ffcdd2
    style B1 fill:#ffcdd2
    style C1 fill:#ffcdd2
```

## Configuración de Endpoints Niubiz

### Ambientes Disponibles

| Ambiente | Base URL | Propósito |
|----------|----------|-----------|
| **QA/Sandbox** | `https://apisandbox.vnforapps.com` | Desarrollo y pruebas |
| **Producción** | `https://apiprod.vnforapps.com` | Transacciones reales |

### Endpoints Específicos

#### 1. Security Token
- **URL**: `/api.security/v1/security`
- **Método**: POST
- **Propósito**: Autenticación inicial

```json
POST /api.security/v1/security
Content-Type: application/json

{
  "merchantId": "123456789",
  "username": "tu_usuario",
  "password": "tu_password"
}
```

#### 2. Session Creation
- **URL**: `/api.ecommerce/v2/ecommerce/token/session/{merchantId}`
- **Método**: POST
- **Propósito**: Crear sesión de pago

```json
POST /api.ecommerce/v2/ecommerce/token/session/123456789
Authorization: Bearer {security_token}
Content-Type: application/json

{
  "amount": 99.99,
  "antifraud": {
    "clientIp": "192.168.1.100",
    "merchantDefineData": {
      "MDD4": "cliente@email.com",
      "MDD21": "0",
      "MDD32": "231215143022",
      "MDD75": "Registrado",
      "MDD77": "999"
    }
  },
  "channel": "web"
}
```

#### 3. Authorization
- **URL**: `/api.authorization/v3/authorization/ecommerce/{merchantId}`
- **Método**: POST
- **Propósito**: Autorizar transacción

```json
POST /api.authorization/v3/authorization/ecommerce/123456789
Authorization: Bearer {security_token}
Content-Type: application/json

{
  "antifraud": null,
  "captureType": "manual",
  "channel": "web",
  "countable": true,
  "order": {
    "amount": 99.99,
    "currency": "PEN",
    "purchaseNumber": "231215143022",
    "tokenId": "transaction_token_from_sdk"
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

## Códigos de Respuesta Niubiz

### Action Codes Principales

| Código | Descripción | Acción Recomendada |
|--------|-------------|-------------------|
| `000` | ✅ Transacción aprobada | Procesar como exitosa |
| `101` | ❌ Tarjeta expirada | Solicitar tarjeta válida |
| `102` | ❌ Tarjeta restringida | Contactar banco |
| `104` | ❌ Monto no permitido | Verificar límites |
| `106` | ❌ PIN incorrecto | Reintentar |
| `107` | ❌ Tarjeta no válida | Verificar datos |
| `201` | ❌ Tarjeta rechazada | Usar otra tarjeta |
| `202` | ❌ Tarjeta robada | Bloquear y reportar |
| `203` | ❌ Tarjeta perdida | Bloquear y reportar |
| `291` | ⚠️ Banco no disponible | Reintentar más tarde |
| `999` | ⚠️ Error del sistema | Contactar soporte |

### HTTP Status Codes

| Status | Significado | Manejo en la App |
|--------|-------------|------------------|
| `200` | OK | Procesar response |
| `400` | Bad Request | Validar parámetros |
| `401` | Unauthorized | Renovar token |
| `403` | Forbidden | Verificar permisos |
| `500` | Internal Server Error | Reintentar o fallar |
| `503` | Service Unavailable | Reintentar más tarde |

## Configuración de Timeouts

### Timeouts Recomendados

```csharp
services.AddHttpClient<NiubizClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2); // Total timeout
});

// En NiubizClient
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var response = await _httpClient.SendAsync(request, cts.Token);
```

### Timeouts por Endpoint

| Endpoint | Timeout Recomendado | Razón |
|----------|-------------------|-------|
| Security Token | 15 segundos | Autenticación rápida |
| Create Session | 20 segundos | Procesos antifraude |
| Authorization | 45 segundos | Validación bancaria |

## Manejo de Errores y Retry Logic

### Estrategia de Reintentos

```mermaid
flowchart TD
    A[API Call] --> B{Success?}
    B -->|Yes| C[Return Result]
    B -->|No| D{Retryable Error?}
    D -->|No| E[Throw Exception]
    D -->|Yes| F{Retry Count < Max?}
    F -->|No| G[Throw TimeoutException]
    F -->|Yes| H[Wait with Backoff]
    H --> I[Increment Counter]
    I --> A

    style C fill:#c8e6c9
    style E fill:#ffcdd2
    style G fill:#ffcdd2
```

### Errores que Permiten Retry

- `HttpRequestException` (problemas de red)
- `TaskCanceledException` (timeout)
- HTTP 500, 502, 503, 504
- HTTP 429 (Rate limiting)

### Errores que NO Permiten Retry

- HTTP 400 (Bad Request)
- HTTP 401 (Unauthorized)
- HTTP 403 (Forbidden)
- HTTP 404 (Not Found)

## Configuración de Logging

### Niveles de Log por Componente

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "IntegracionNiubizDemo.Infrastructure.Niubiz.NiubizClient": "Debug",
      "IntegracionNiubizDemo.Application.Services.CheckoutService": "Information",
      "System.Net.Http.HttpClient": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Eventos de Log Importantes

```csharp
// Nivel Debug - Detalles técnicos
_logger.LogDebug("Enviando request a Niubiz: {Endpoint} - Body: {RequestBody}", endpoint, requestBody);

// Nivel Information - Flujo de negocio
_logger.LogInformation("Pago iniciado para producto {ProductId} por monto {Amount} {Currency}", productId, amount, currency);

// Nivel Warning - Situaciones recuperables
_logger.LogWarning("Pago rechazado para compra {PurchaseNumber} - Código: {ActionCode}", purchaseNumber, actionCode);

// Nivel Error - Errores que requieren intervención
_logger.LogError(ex, "Error de comunicación con Niubiz para compra {PurchaseNumber}", purchaseNumber);
```

## Métricas y Monitoreo

### KPIs Importantes

1. **Tasa de Éxito**: % de pagos aprobados vs iniciados
2. **Tiempo de Respuesta**: Latencia promedio de APIs Niubiz
3. **Rate de Error**: % de errores técnicos
4. **Abandono**: % de sesiones iniciadas vs completadas

### Health Checks

```csharp
services.AddHealthChecks()
    .AddCheck<NiubizHealthCheck>("niubiz-api")
    .AddDbContextCheck<AppDbContext>("database");
```

Este flujo documenta completamente el proceso de integración con Niubiz, desde la perspectiva técnica hasta la experiencia del usuario.