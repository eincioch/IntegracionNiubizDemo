# Integración Niubiz Demo

**Manual de Integración Completo para Desarrolladores Terceros**

## Índice

1. [Descripción General](#descripción-general)
2. [Arquitectura del Proyecto](#arquitectura-del-proyecto)
3. [Diagrama de Dependencias](#diagrama-de-dependencias)
4. [API Niubiz - Documentación Completa](#api-niubiz---documentación-completa)
5. [Configuración y Variables de Entorno](#configuración-y-variables-de-entorno)
6. [Instalación y Ejecución](#instalación-y-ejecución)
7. [Endpoints y Funcionalidades](#endpoints-y-funcionalidades)
8. [Flujo de Pagos](#flujo-de-pagos)
9. [Ejemplos de Uso](#ejemplos-de-uso)
10. [Guía para Desarrolladores Terceros](#guía-para-desarrolladores-terceros)
11. [Estructura de Datos](#estructura-de-datos)
12. [Manejo de Errores](#manejo-de-errores)
13. [Seguridad](#seguridad)

---

## Descripción General

**IntegracionNiubizDemo** es una aplicación web desarrollada en **.NET 8.0** que demuestra la integración completa con la **API de pagos de Niubiz (VisaNet)**. La aplicación implementa un **sistema de e-commerce** con funcionalidades de catálogo de productos y procesamiento de pagos seguro.

### Características Principales

- ✅ **Arquitectura Clean Architecture** con separación clara de responsabilidades
- ✅ **Integración completa con Niubiz API** para procesamiento de pagos (security token, sesión y autorización e-commerce v3)
- ✅ **Captura de dirección/ubicación del comprador** (formulario manual o geolocalización del navegador + reverse geocoding) para alimentar el `dataMap` antifraude de Niubiz
- ✅ **Interfaz web responsiva** para gestión de productos y checkout
- ✅ **Base de datos SQLite** para persistencia de datos
- ✅ **Manejo de transacciones** con seguimiento completo del estado
- ✅ **Sistema de configuración flexible** para múltiples ambientes (QA/Producción)

### Tecnologías Utilizadas

- **Framework**: .NET 8.0 (ASP.NET Core MVC)
- **Base de Datos**: SQLite con Entity Framework Core 9.0.8
- **Frontend**: HTML5, CSS3, JavaScript (Bootstrap 5), jQuery Validation Unobtrusive
- **Patrones de Diseño**: Clean Architecture, Repository Pattern, Dependency Injection
- **APIs Externas**: Niubiz Payment Gateway API, Google Maps Geocoding API (reverse geocoding opcional)

---

## Arquitectura del Proyecto

El proyecto sigue los principios de **Clean Architecture** con una clara separación en capas:

```
IntegracionNiubizDemo/
├── 📁 IntegracionNiubizDemo.Domain/          # Capa de Dominio
│   └── Entities/                             # Entidades del negocio
│       ├── Product.cs                        # Producto
│       ├── Order.cs                          # Orden de compra (incluye datos de dirección del cliente)
│       └── PaymentTransaction.cs             # Transacción de pago
│
├── 📁 IntegracionNiubizDemo.Application/     # Capa de Aplicación
│   ├── Abstractions/                         # Interfaces y contratos
│   │   ├── IProductService.cs                # Servicio de productos
│   │   ├── ICheckoutService.cs               # Servicio de checkout
│   │   ├── INiubizGateway.cs                 # Gateway de Niubiz
│   │   ├── IGeocodingService.cs              # Abstracción de reverse geocoding
│   │   ├── IProductRepository.cs             # Repositorio de productos
│   │   ├── IOrderRepository.cs               # Repositorio de órdenes
│   │   └── IPaymentRepository.cs             # Repositorio de pagos
│   ├── Services/                             # Servicios de aplicación
│   │   ├── ProductService.cs                 # Lógica de productos
│   │   └── CheckoutService.cs                # Lógica de checkout
│   ├── Dtos/                                 # Objetos de transferencia
│   │   └── CheckoutDtos.cs                   # DTOs de checkout, dirección y geocoding
│   └── DependencyInjection.cs               # Configuración DI
│
├── 📁 IntegracionNiubizDemo.Persistence/    # Capa de Persistencia
│   ├── Data/
│   │   ├── AppDbContext.cs                   # Contexto de EF Core
│   │   └── SchemaUpgrader.cs                 # Upgrader idempotente de esquema SQLite (sin migraciones EF)
│   └── Repositories/                         # Implementación de repositorios
│       ├── ProductRepository.cs              # Repositorio de productos
│       ├── OrderRepository.cs                # Repositorio de órdenes
│       └── PaymentRepository.cs              # Repositorio de pagos
│
├── 📁 IntegracionNiubizDemo.Infrastructure/ # Capa de Infraestructura
│   ├── Niubiz/                              # Integración con Niubiz
│   │   ├── NiubizClient.cs                  # Cliente HTTP para Niubiz
│   │   └── NiubizOptions.cs                 # Configuración de Niubiz (incluye ubicación del comercio)
│   ├── Geocoding/                           # Reverse geocoding (Google Maps)
│   │   ├── GoogleGeocodingService.cs        # Implementación de IGeocodingService
│   │   └── GoogleMapsOptions.cs             # Configuración de Google Maps Geocoding API
│   └── DependencyInjection.cs              # Configuración DI
│
└── 📁 IntegracionNiubizDemo.Web/            # Capa de Presentación
    ├── Controllers/                          # Controladores MVC
    │   ├── ProductsController.cs             # Gestión de productos
    │   ├── CheckoutController.cs             # Proceso de checkout (formulario, reverse-geocode, confirm)
    │   └── HomeController.cs                 # Página principal
    ├── Models/                               # ViewModels y DTOs web
    │   ├── CheckoutPayViewModel.cs           # Formulario de checkout + datos de sesión Niubiz
    │   └── ReverseGeocodeRequest.cs          # Request AJAX del botón "Usar mi ubicación"
    ├── Views/                                # Vistas Razor
    │   ├── Products/                         # Vistas de productos
    │   ├── Checkout/                         # Vistas de checkout
    │   └── Shared/                           # Vistas compartidas
    ├── appsettings.json                      # Configuración base
    ├── appsettings.Development.json          # Configuración desarrollo (Niubiz + GoogleMaps)
    └── Program.cs                            # Punto de entrada (incluye SchemaUpgrader al inicio)
```

### Principios Aplicados

1. **Separación de Responsabilidades**: Cada capa tiene una responsabilidad específica
2. **Inversión de Dependencias**: Las capas externas dependen de las internas mediante interfaces
3. **Independencia de Frameworks**: La lógica de negocio no depende de frameworks externos
4. **Testabilidad**: Arquitectura preparada para pruebas unitarias e integración

---

## Diagrama de Dependencias

```mermaid
graph TD
    A[IntegracionNiubizDemo.Web] --> B[IntegracionNiubizDemo.Application]
    A --> C[IntegracionNiubizDemo.Infrastructure]
    
    B --> D[IntegracionNiubizDemo.Domain]
    
    C --> E[IntegracionNiubizDemo.Persistence]
    C --> B
    C --> D
    
    E --> D
    E --> B

    subgraph "Capas"
        A["🎨 Presentación<br/>Controllers, Views, DTOs"]
        B["💼 Aplicación<br/>Services, Interfaces"]
        C["🔧 Infraestructura<br/>Niubiz Client, DI"]
        E["💾 Persistencia<br/>DbContext, Repositories"]
        D["🏛️ Dominio<br/>Entities, Business Logic"]
    end

    subgraph "APIs Externas"
        F[Niubiz API]
        G[SQLite Database]
        H[Google Maps Geocoding API]
    end

    C -.-> F
    C -.-> H
    E -.-> G
```

### Dependencias de Paquetes NuGet

**Capa Application:**
- `Microsoft.Extensions.DependencyInjection` (9.0.8)
- `Microsoft.Extensions.Configuration.Abstractions` (9.0.8)

**Capa Infrastructure:**
- `Microsoft.EntityFrameworkCore` (9.0.8)
- `Microsoft.EntityFrameworkCore.Sqlite` (9.0.8)
- `Microsoft.EntityFrameworkCore.InMemory` (9.0.8)
- `Microsoft.Extensions.Http` (9.0.8)
- `Microsoft.Extensions.Logging.Abstractions` (9.0.8)
- `Microsoft.Extensions.Options` (9.0.8)
- `Microsoft.Extensions.Configuration.Abstractions` (9.0.8)

**Capa Persistence:**
- `Microsoft.EntityFrameworkCore` (9.0.8)
- `Microsoft.EntityFrameworkCore.Relational` (9.0.8) — requerido por `SchemaUpgrader` (`DatabaseFacade.GetDbConnection()`)
- `Microsoft.EntityFrameworkCore.Design` (9.0.8)

---

## API Niubiz - Documentación Completa

### Descripción de la API

**Niubiz** (anteriormente VisaNet) es la plataforma de pagos electrónicos líder en Perú. La integración se realiza mediante una API REST que sigue un flujo de 3 pasos:

1. **Obtener Security Token** - Autenticación inicial
2. **Crear Session** - Inicializar transacción
3. **Autorizar Pago** - Procesar la transacción

### URLs Base por Ambiente

| Ambiente | URL Base | Descripción |
|----------|----------|-------------|
| **QA/Testing** | `https://apitestenv.vnforapps.com` | Ambiente de pruebas |
| **Producción** | `https://apiprod.vnforapps.com` | Ambiente productivo |

### Endpoints de la API

#### 1. Security Token Endpoint

**Propósito**: Obtener token de seguridad para autenticación

```http
POST /api.security/v1/security
Authorization: Basic {base64(username:password)}
Content-Type: text/plain
```

**Respuesta**:
```json
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

#### 2. Session Endpoint

**Propósito**: Crear sesión de pago e inicializar transacción

```http
POST /api.ecommerce/v2/ecommerce/token/session/{merchantId}
Authorization: {securityToken}
Content-Type: application/json
```

**Cuerpo de la Petición**:
```json
{
  "channel": "web",
  "amount": "150.00",
  "antifraud": {
    "clientIp": "127.0.0.1",
    "merchantDefineData": {
      "MDD4": "cliente@email.com",
      "MDD30": "40904759",
      "MDD31": "986687645",
      "MDD32": "40904759",
      "MDD33": "25",
      "MDD34": "40904759",
      "MDD63": "25",
      "MDD65": "40904759",
      "MDD71": "700526895"
    }
  }
}
```

**Respuesta**:
```json
{
  "sessionKey": "b25b9b7c-4a83-4a5e-8b5c-9d7e8f9g0h1i",
  "expirationTime": 1200,
  "merchantId": "400000181"
}
```

#### 3. Authorization Endpoint

**Propósito**: Autorizar el pago con los datos de la tarjeta

```http
POST /api.authorization/v3/authorization/ecommerce/{merchantId}
Authorization: {securityToken}
Content-Type: application/json
```

**Cuerpo de la Petición**:
```json
{
  "channel": "web",
  "captureType": "automatic",
  "countable": true,
  "order": {
    "tokenId": "{transactionToken}",
    "purchaseNumber": "PUR12345",
    "amount": 150.00,
    "currency": "PEN"
  }
}
```

**Respuesta Exitosa**:
```json
{
  "order": {
    "actionCode": "000",
    "authorizationCode": "123456",
    "purchaseNumber": "PUR12345",
    "amount": 150.00,
    "currency": "PEN"
  },
  "dataMap": {
    "CARD": "**** **** **** 1234",
    "BRAND": "visa"
  }
}
```

### Configuración de Niubiz

La integración requiere las siguientes credenciales:

```json
{
  "Niubiz": {
    "Environment": "qa",           // "qa" | "prod"
    "MerchantId": "400000181",     // ID del comercio
    "Username": "usuario@merchant.com",
    "Password": "password123",
    "Currency": "PEN",             // Moneda (PEN/USD)
    "MerchantUrl": "http://localhost:5110", // dataMap.urlAddress (Autorización)
    "MerchantCityName": "Lima",                    // dataMap.serviceLocationCityName
    "MerchantCountrySubdivisionCode": "LIM",       // dataMap.serviceLocationCountrySubdivisionCode
    "MerchantCountryCode": "PE",                   // dataMap.serviceLocationCountryCode
    "MerchantPostalCode": "15001",                 // dataMap.serviceLocationPostalCode
    "BaseUrls": {
      "qa": "https://apitestenv.vnforapps.com",
      "prod": "https://apiprod.vnforapps.com"
    },
    "StaticContent": {
      "qa": "https://static-content-qas.vnforapps.com/v2/js/checkout.js?qa=true",
      "prod": "https://static-content.vnforapps.com/v2/js/checkout.js"
    },
    "Endpoints": {
      "Security": "/api.security/v1/security",
      "Session": "/api.ecommerce/v2/ecommerce/token/session/{merchantId}",
      "Authorization": "/api.authorization/v3/authorization/ecommerce/{merchantId}"
    }
  },
  "GoogleMaps": {
    "ApiKey": "",                  // Requerida solo para el botón "Usar mi ubicación" (reverse geocoding)
    "GeocodeBaseUrl": "https://maps.googleapis.com/maps/api/geocode/json",
    "Language": "es"
  }
}
```

> `MerchantCityName`, `MerchantCountrySubdivisionCode`, `MerchantCountryCode` y `MerchantPostalCode` describen la ubicación del **comercio** y se envían en `dataMap` de la Autorización. Los datos del **comprador** (`cardholderCity`, `cardholderCountry`, `cardholderAddress`, `cardholderPostalCode`, `cardholderState`) se capturan en el formulario de checkout y viajan en `dataMap` de la Sesión (ver [Estructura de Datos](#estructura-de-datos)).
    "Endpoints": {
      "Security": "/api.security/v1/security",
      "Session": "/api.ecommerce/v2/ecommerce/token/session/{merchantId}",
      "Authorization": "/api.authorization/v3/authorization/ecommerce/{merchantId}"
    }
  }
}
```

### Implementación en el Proyecto

La integración está encapsulada en la clase `NiubizClient`:

```csharp
public class NiubizClient : INiubizGateway
{
    public async Task<string> GetSecurityTokenAsync(CancellationToken ct = default);
    public async Task<string> CreateSessionAsync(string securityToken, decimal amount,
        string purchaseNumber, string currency, CustomerAddressInput address, CancellationToken ct = default);
    public async Task<AuthorizationResult> AuthorizeAsync(string securityToken,
        string transactionToken, decimal amount, string currency,
        string purchaseNumber, CancellationToken ct = default);
}
```

`CustomerAddressInput` (dirección, ciudad, departamento/estado, país ISO-2, código postal y teléfono del comprador) se captura en el formulario de checkout y se usa para poblar `dataMap.cardholderCity`, `cardholderCountry`, `cardholderAddress`, `cardholderPostalCode`, `cardholderState` y `cardholderPhoneNumber` al crear la sesión. La autorización, en cambio, usa `NiubizOptions` (ubicación del comercio) para `dataMap.serviceLocation*` y `urlAddress`.

---

## Configuración y Variables de Entorno

### Configuración Base (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Configuración de Desarrollo (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Niubiz": {
    "Environment": "qa",
    "MerchantId": "400000181",
    "Username": "integraciones.visanet@necomplus.com",
    "Password": "d5e7nk$M",
    "Currency": "PEN",
    "MerchantUrl": "http://localhost:5110",
    "MerchantCityName": "Lima",
    "MerchantCountrySubdivisionCode": "LIM",
    "MerchantCountryCode": "PE",
    "MerchantPostalCode": "15001",
    "BaseUrls": {
      "qa": "https://apitestenv.vnforapps.com",
      "prod": "https://apiprod.vnforapps.com"
    },
    "StaticContent": {
      "qa": "https://static-content-qas.vnforapps.com/v2/js/checkout.js?qa=true",
      "prod": "https://static-content.vnforapps.com/v2/js/checkout.js"
    },
    "Endpoints": {
      "Security": "/api.security/v1/security",
      "Session": "/api.ecommerce/v2/ecommerce/token/session/{merchantId}",
      "Authorization": "/api.authorization/v3/authorization/ecommerce/{merchantId}"
    }
  },
  "GoogleMaps": {
    "ApiKey": "",
    "GeocodeBaseUrl": "https://maps.googleapis.com/maps/api/geocode/json",
    "Language": "es"
  },
  "ConnectionStrings": {
    "Default": "Data Source=app.db"
  }
}
```

### Variables de Entorno de Producción

Para producción, configure las siguientes variables de entorno:

```bash
# Configuración Niubiz
NIUBIZ__ENVIRONMENT=prod
NIUBIZ__MERCHANTID=su_merchant_id_produccion
NIUBIZ__USERNAME=su_usuario_produccion
NIUBIZ__PASSWORD=su_password_produccion

# Base de datos
CONNECTIONSTRINGS__DEFAULT="Data Source=production.db"

# Logging
LOGGING__LOGLEVEL__DEFAULT=Warning
```

### User Secrets para Desarrollo

Para mayor seguridad en desarrollo, use User Secrets:

```bash
dotnet user-secrets init --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:MerchantId" "su_merchant_id" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Username" "su_usuario" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Password" "su_password" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "GoogleMaps:ApiKey" "su_google_maps_api_key" --project IntegracionNiubizDemo.Web
```

> Los User Secrets tienen **mayor prioridad** que `appsettings.Development.json` y sobrescriben sus valores en tiempo de ejecución. Si las transacciones usan un `MerchantId` inesperado, revise primero los User Secrets del proyecto Web.
>
> `GoogleMaps:ApiKey` es **opcional**: solo se usa para el botón "Usar mi ubicación" (reverse geocoding). Sin ella, el usuario puede completar la dirección manualmente.

---

## Instalación y Ejecución

### Requisitos Previos

- **.NET 8.0 SDK** o superior
- **Git** para clonar el repositorio
- **IDE** recomendado: Visual Studio 2022 o Visual Studio Code
- **Navegador web** moderno

### Pasos de Instalación

1. **Clonar el repositorio**:
```bash
git clone https://github.com/eincioch/IntegracionNiubizDemo.git
cd IntegracionNiubizDemo
```

2. **Restaurar paquetes NuGet**:
```bash
dotnet restore
```

3. **Compilar la solución**:
```bash
dotnet build
```

4. **Configurar credenciales de Niubiz**:
   - Editar `IntegracionNiubizDemo.Web/appsettings.Development.json`
   - O configurar User Secrets (recomendado)

5. **Ejecutar la aplicación**:
```bash
cd IntegracionNiubizDemo.Web
dotnet run
```

6. **Abrir en el navegador**:
   - URL: `https://localhost:7269` o `http://localhost:5110`

### Scripts Útiles

**Compilar y ejecutar**:
```bash
dotnet build && cd IntegracionNiubizDemo.Web && dotnet run
```

**Limpiar y recompilar**:
```bash
dotnet clean && dotnet build
```

**Ejecutar en modo desarrollo con watch**:
```bash
cd IntegracionNiubizDemo.Web
dotnet watch run
```

---

## Endpoints y Funcionalidades

### Endpoints Públicos

| Método | Ruta | Descripción | Parámetros |
|--------|------|-------------|------------|
| `GET` | `/` | Página principal | - |
| `GET` | `/Products` | Lista de productos | - |
| `GET` | `/checkout/pay/{productId}` | Muestra el formulario de dirección/checkout | `productId: Guid`, `email?: string` |
| `POST` | `/checkout/pay/{productId}` | Valida dirección e inicia sesión Niubiz | `productId: Guid`, formulario `CheckoutPayViewModel` |
| `POST` | `/checkout/reverse-geocode` | Resuelve dirección a partir de lat/lng (botón "Usar mi ubicación") | JSON `{ latitude, longitude }` |
| `POST` | `/checkout/confirm` | Confirmar pago | `purchaseNumber, transactionToken` (o `tokenId`/`token`) |

### API Controllers

#### ProductsController

**Funcionalidades**:
- Listar productos disponibles
- Navegación hacia proceso de checkout

**Métodos**:
```csharp
public async Task<IActionResult> Index() // Lista productos
```

#### CheckoutController

**Funcionalidades**:
- Mostrar formulario de dirección/ubicación del comprador y datos del producto
- Inicializar la sesión Niubiz una vez validado el formulario
- Resolver dirección mediante reverse geocoding (botón "Usar mi ubicación")
- Confirmar transacción con Niubiz
- Manejo de respuestas y errores

**Métodos**:
```csharp
[HttpGet("/checkout/pay/{productId:guid}")]
public async Task<IActionResult> Pay(Guid productId, string? email = null, CancellationToken ct = default)

[HttpPost("/checkout/pay/{productId:guid}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Pay(Guid productId, CheckoutPayViewModel model, CancellationToken ct = default)

[HttpPost("/checkout/reverse-geocode")]
public async Task<IActionResult> ReverseGeocode([FromBody] ReverseGeocodeRequest request, CancellationToken ct = default)

[HttpPost("/checkout/confirm")]
public async Task<IActionResult> Confirm(
    [FromForm] string? purchaseNumber,
    [FromForm] string? transactionToken,
    [FromForm(Name = "tokenId")] string? tokenId,
    [FromForm(Name = "token")] string? token,
    CancellationToken ct = default)
```

### Servicios de Aplicación

#### ICheckoutService

```csharp
public interface ICheckoutService
{
    Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail, CustomerAddressInput address, CancellationToken ct = default);
    Task<ConfirmResult> ConfirmAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default);
}
```

#### IProductService

```csharp
public interface IProductService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task EnsureSeedDataAsync(CancellationToken ct = default);
}
```

#### IGeocodingService

```csharp
public interface IGeocodingService
{
    Task<ReverseGeocodeResult> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken ct = default);
}
```

---

## Flujo de Pagos

### Diagrama de Flujo Completo

```mermaid
sequenceDiagram
    participant U as Usuario
    participant W as Web App
    participant G as GeocodingService
    participant C as CheckoutService
    participant N as NiubizClient
    participant API as Niubiz API
    participant DB as Database

    U->>W: 1. Ver productos
    W->>C: 2. GetProductsAsync()
    C->>DB: 3. Consultar productos
    DB-->>C: 4. Lista productos
    C-->>W: 5. Productos
    W-->>U: 6. Mostrar catálogo

    U->>W: 7. GET /checkout/pay/{productId}
    W-->>U: 8. Formulario de dirección/ubicación

    opt Usar mi ubicación
        U->>W: 9. Geolocalización del navegador (lat/lng)
        W->>G: 10. ReverseGeocodeAsync(lat, lng)
        G-->>W: 11. Dirección normalizada
        W-->>U: 12. Autocompletar formulario
    end

    U->>W: 13. Enviar formulario (POST /checkout/pay/{productId})
    W->>C: 14. InitAsync(productId, email, CustomerAddressInput)
    C->>DB: 15. Crear Order (incluye dirección del cliente)
    C->>N: 16. GetSecurityTokenAsync()
    N->>API: 17. POST /security
    API-->>N: 18. Security Token
    C->>N: 19. CreateSessionAsync(..., address)
    N->>API: 20. POST /session/{merchantId} (dataMap cardholder*)
    API-->>N: 21. Session Key
    C->>DB: 22. Guardar PaymentTransaction
    C-->>W: 23. CheckoutInitResult
    W-->>U: 24. Widget de pago Niubiz (checkout.js)

    U->>W: 25. Completar datos tarjeta
    W->>C: 26. ConfirmAsync(purchaseNumber, token)
    C->>N: 27. AuthorizeAsync()
    N->>API: 28. POST /authorization/{merchantId} (dataMap serviceLocation*)
    API-->>N: 29. Authorization Result
    C->>DB: 30. Actualizar Order/Payment
    C-->>W: 31. ConfirmResult
    W-->>U: 32. Resultado final
```

### Descripción Detallada del Flujo

#### Fase 1: Captura de Dirección y Datos del Comprador

1. **Usuario selecciona producto**: Navega desde el catálogo
2. **Formulario de checkout**: Se muestra con dirección, ciudad, departamento/estado, país (ISO-2), código postal y teléfono
3. **(Opcional) Geolocalización**: El botón "Usar mi ubicación" obtiene lat/lng del navegador y llama a `/checkout/reverse-geocode`, que usa `IGeocodingService` (Google Maps) para autocompletar el formulario
4. **Validación**: DataAnnotations en `CheckoutPayViewModel` valida los campos en cliente y servidor

#### Fase 2: Inicialización del Checkout

5. **Validación de producto**: Verificar existencia y precio
6. **Creación de orden**: Generar número de compra único y persistir la dirección capturada en `Order`
7. **Obtención de Security Token**: Autenticación con Niubiz
8. **Creación de sesión**: `CreateSessionAsync` envía `dataMap.cardholder*` con los datos reales del comprador para el motor antifraude
9. **Persistencia**: Guardar `PaymentTransaction` con la `SessionKey`

#### Fase 3: Captura de Datos de Pago

10. **Widget de pago**: Se carga el JavaScript de Niubiz (`checkout.js`) con la `SessionKey`
11. **Tokenización**: Niubiz tokeniza los datos de tarjeta en el navegador (nunca tocan el servidor propio)
12. **Validación**: Niubiz valida los datos en el frontend

#### Fase 4: Autorización y Confirmación

13. **Envío de token**: Submit del formulario con `transactionToken`
14. **Autorización**: `AuthorizeAsync` procesa el pago enviando `dataMap.serviceLocation*` (ubicación del comercio, desde `NiubizOptions`)
15. **Actualización de estado**: Marcar orden como pagada/rechazada
16. **Respuesta al usuario**: Mostrar resultado final en `Confirm.cshtml`

### Estados de Orden

```csharp
public enum OrderStatus 
{ 
    Pending = 0,    // Orden creada, esperando pago
    Paid = 1,       // Pago autorizado exitosamente
    Rejected = 2,   // Pago rechazado
    Error = 3       // Error en el proceso
}
```

---

## Ejemplos de Uso

### Ejemplo 1: Consultar Productos

**Request**:
```http
GET /Products
Accept: text/html
```

**Response**: Página HTML con lista de productos

### Ejemplo 2: Checkout con Captura de Dirección

**Request (mostrar formulario)**:
```http
GET /checkout/pay/550e8400-e29b-41d4-a716-446655440000?email=cliente@email.com
```

**Response**: Formulario HTML con campos de dirección/ubicación (país por defecto `PE`)

**Request (enviar formulario e iniciar sesión Niubiz)**:
```http
POST /checkout/pay/550e8400-e29b-41d4-a716-446655440000
Content-Type: application/x-www-form-urlencoded

CustomerEmail=cliente@email.com&Address=Av. Siempre Viva 123&City=Lima&State=LIM&Country=PE&PostalCode=15001&Phone=987654321
```

**Response**: Misma vista con el widget de pago de Niubiz (`checkout.js`) inicializado con la `SessionKey`

### Ejemplo 3: Reverse Geocoding ("Usar mi ubicación")

**Request**:
```http
POST /checkout/reverse-geocode
Content-Type: application/json

{ "latitude": -12.0464, "longitude": -77.0428 }
```

**Response**:
```json
{
  "success": true,
  "address": "Av. Siempre Viva 123",
  "city": "Lima",
  "state": "LIM",
  "country": "PE",
  "postalCode": "15001",
  "error": null
}
```

### Ejemplo 4: Confirmar Pago

**Request**:
```http
POST /checkout/confirm
Content-Type: application/x-www-form-urlencoded

purchaseNumber=PUR123456789&transactionToken=abc123def456
```

**Response**: Página de confirmación con resultado

### Ejemplo 5: Integración vía API (Para Terceros)

Si desea integrar con otra aplicación, puede usar los servicios directamente:

```csharp
// Inyectar dependencias
public class MiControlador
{
    private readonly ICheckoutService _checkout;

    public MiControlador(ICheckoutService checkout)
    {
        _checkout = checkout;
    }

    // Inicializar pago (requiere la dirección/ubicación del comprador)
    public async Task<ActionResult> IniciarPago(Guid productId)
    {
        var address = new CustomerAddressInput(
            Address: "Av. Siempre Viva 123",
            City: "Lima",
            State: "LIM",
            Country: "PE",
            PostalCode: "15001",
            Phone: "987654321");

        var result = await _checkout.InitAsync(productId, "cliente@email.com", address);

        return Json(new {
            sessionKey = result.SessionKey,
            merchantId = result.MerchantId,
            purchaseNumber = result.PurchaseNumber,
            amount = result.Amount,
            currency = result.Currency,
            jsUrl = result.StaticJsUrl
        });
    }
    
    // Confirmar pago
    public async Task<ActionResult> ConfirmarPago(string purchaseNumber, string transactionToken)
    {
        var result = await _checkout.ConfirmAsync(purchaseNumber, transactionToken);
        
        return Json(new {
            success = result.Success,
            authorizationCode = result.AuthorizationCode,
            message = result.Message
        });
    }
}
```

---

## Guía para Desarrolladores Terceros

### Integración Básica

#### Paso 1: Configurar Dependencias

Agregue el servicio a su aplicación:

```csharp
// En Program.cs o Startup.cs
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

#### Paso 2: Configurar Niubiz

```json
{
  "Niubiz": {
    "Environment": "qa",
    "MerchantId": "SU_MERCHANT_ID",
    "Username": "SU_USUARIO",
    "Password": "SU_PASSWORD",
    "Currency": "PEN"
  }
}
```

#### Paso 3: Implementar Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly ICheckoutService _checkout;
    
    public PagosController(ICheckoutService checkout)
    {
        _checkout = checkout;
    }
    
    [HttpPost("iniciar")]
    public async Task<IActionResult> IniciarPago([FromBody] IniciarPagoRequest request)
    {
        try
        {
            var address = new CustomerAddressInput(
                request.Address, request.City, request.State, request.Country,
                request.PostalCode, request.Phone);

            var result = await _checkout.InitAsync(request.ProductId, request.Email, address);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirmar")]
    public async Task<IActionResult> ConfirmarPago([FromBody] ConfirmarPagoRequest request)
    {
        try
        {
            var result = await _checkout.ConfirmAsync(request.PurchaseNumber, request.TransactionToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record IniciarPagoRequest(
    Guid ProductId, string? Email,
    string Address, string City, string State, string Country, string PostalCode, string? Phone);
public record ConfirmarPagoRequest(string PurchaseNumber, string TransactionToken);
```

### Integración Avanzada

#### Personalizar Repositorios

```csharp
public class MiProductRepository : IProductRepository
{
    // Implementar interfaz con su lógica personalizada
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Su implementación personalizada
    }
}

// Registrar en DI
builder.Services.AddScoped<IProductRepository, MiProductRepository>();
```

#### Extender Servicios

```csharp
public class MiCheckoutService : ICheckoutService
{
    private readonly CheckoutService _baseService;
    
    public MiCheckoutService(CheckoutService baseService)
    {
        _baseService = baseService;
    }
    
    public async Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail, CustomerAddressInput address, CancellationToken ct = default)
    {
        // Lógica personalizada antes
        var result = await _baseService.InitAsync(productId, customerEmail, address, ct);
        // Lógica personalizada después
        return result;
    }
}
```

#### Frontend JavaScript

Para implementar el frontend, use el JavaScript de Niubiz:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Checkout</title>
</head>
<body>
    <form id="payment-form" action="/api/pagos/confirmar" method="post">
        <input type="hidden" name="purchaseNumber" value="PUR123456" />
        <input type="hidden" id="transactionToken" name="transactionToken" />
    </form>

    <script type="text/javascript" 
            src="https://static-content-qas.vnforapps.com/v2/js/checkout.js?qa=true"
            data-sessiontoken="session_key_from_backend"
            data-channel="web"
            data-merchantid="400000181"
            data-purchasenumber="PUR123456"
            data-amount="150.00"
            data-currency="PEN"
            data-expirationminutes="20"
            data-timeouturl="about:blank">
    </script>

    <script>
        function culqiSuccessCallback(token) {
            document.getElementById('transactionToken').value = token.token;
            document.getElementById('payment-form').submit();
        }
    </script>
</body>
</html>
```

---

## Estructura de Datos

### Entidades del Dominio

#### Product

```csharp
public class Product
{
    public Guid Id { get; set; }          // Identificador único
    public string Name { get; set; }      // Nombre del producto
    public decimal Price { get; set; }    // Precio en la moneda configurada
}
```

#### Order

```csharp
public class Order
{
    public Guid Id { get; set; }                    // Identificador único
    public string PurchaseNumber { get; set; }      // Número de compra único
    public decimal Amount { get; set; }             // Monto total
    public string Currency { get; set; }            // Moneda (PEN/USD)
    public DateTimeOffset CreatedAt { get; set; }   // Fecha de creación
    public OrderStatus Status { get; set; }         // Estado de la orden
    public string? CustomerEmail { get; set; }      // Email del cliente

    // Dirección/ubicación del comprador, requerida por Niubiz (dataMap) para
    // el motor antifraude de la sesión. Se capturan en el formulario de checkout.
    public string? CustomerAddress { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerState { get; set; }         // Departamento/región (ej. "LIM")
    public string? CustomerCountry { get; set; } = "PE"; // ISO 3166-1 alpha-2
    public string? CustomerPostalCode { get; set; }
    public string? CustomerPhone { get; set; }
}
```

#### PaymentTransaction

```csharp
public class PaymentTransaction
{
    public Guid Id { get; set; }                    // Identificador único
    public Guid OrderId { get; set; }               // Referencia a la orden
    public string? SessionKey { get; set; }         // Clave de sesión de Niubiz
    public string? TransactionToken { get; set; }   // Token de transacción
    public string? AuthorizationCode { get; set; }  // Código de autorización
    public string? MaskedCard { get; set; }         // Tarjeta enmascarada
    public string Status { get; set; }              // Estado de la transacción
    public string? RawResponse { get; set; }        // Respuesta cruda de Niubiz
    public DateTimeOffset CreatedAt { get; set; }   // Fecha de creación
}
```

### DTOs de Transferencia

#### CheckoutInitResult

```csharp
public record CheckoutInitResult(
    string MerchantId,        // ID del comercio
    string SessionKey,        // Clave de sesión para el frontend
    string PurchaseNumber,    // Número de compra generado
    decimal Amount,           // Monto a cobrar
    string Currency,          // Moneda
    string StaticJsUrl        // URL del JavaScript de Niubiz
);
```

#### AuthorizationResult

```csharp
public record AuthorizationResult(
    bool Approved,            // Indica si fue aprobado
    string? AuthorizationCode, // Código de autorización
    string? MaskedCard,       // Tarjeta enmascarada
    string RawJson            // Respuesta completa de Niubiz
);
```

#### ConfirmResult

```csharp
public record ConfirmResult(
    bool Success,             // Indica si fue exitoso
    string PurchaseNumber,    // Número de compra
    string? AuthorizationCode, // Código de autorización
    string Message,           // Mensaje para el usuario
    string? MaskedCard,       // Tarjeta enmascarada
    string RawJson            // Respuesta completa
);
```

#### CustomerAddressInput

```csharp
public record CustomerAddressInput(
    string Address,
    string City,
    string State,
    string Country,
    string PostalCode,
    string? Phone
);
```

#### ReverseGeocodeResult

```csharp
public record ReverseGeocodeResult(
    bool Success,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Error
);
```

### Configuración de Base de Datos

Entity Framework está configurado con las siguientes relaciones:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Producto
    modelBuilder.Entity<Product>(b =>
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Name).IsRequired().HasMaxLength(200);
        b.Property(p => p.Price).HasPrecision(18, 2);
    });

    // Orden
    modelBuilder.Entity<Order>(b =>
    {
        b.HasKey(o => o.Id);
        b.Property(o => o.PurchaseNumber).IsRequired().HasMaxLength(20);
        b.HasIndex(o => o.PurchaseNumber).IsUnique();
        b.Property(o => o.Amount).HasPrecision(18, 2);
        b.Property(o => o.Currency).IsRequired().HasMaxLength(3);
        b.Property(o => o.Status).HasConversion<int>();
        b.Property(o => o.CustomerEmail).HasMaxLength(256);
        b.Property(o => o.CustomerAddress).HasMaxLength(300);
        b.Property(o => o.CustomerCity).HasMaxLength(100);
        b.Property(o => o.CustomerState).HasMaxLength(50);
        b.Property(o => o.CustomerCountry).HasMaxLength(2);
        b.Property(o => o.CustomerPostalCode).HasMaxLength(20);
        b.Property(o => o.CustomerPhone).HasMaxLength(20);
    });

    // Transacción de Pago
    modelBuilder.Entity<PaymentTransaction>(b =>
    {
        b.HasKey(t => t.Id);
        b.Property(t => t.SessionKey).HasMaxLength(256);
        b.Property(t => t.TransactionToken).HasMaxLength(256);
        b.Property(t => t.AuthorizationCode).HasMaxLength(64);
        b.Property(t => t.MaskedCard).HasMaxLength(32);
        b.Property(t => t.Status).HasMaxLength(32);
        
        b.HasIndex(t => t.OrderId);
        b.HasOne<Order>()
         .WithMany()
         .HasForeignKey(t => t.OrderId)
         .OnDelete(DeleteBehavior.Cascade);
    });
}
```

> **Nota**: el proyecto no usa migraciones formales de EF Core (`dotnet ef migrations`). En su lugar, `SchemaUpgrader.EnsureOrderAddressColumnsAsync` se ejecuta al inicio de la aplicaci\u00f3n (`Program.cs`) y agrega de forma idempotente, vía `ALTER TABLE`, las columnas de direcci\u00f3n a `Orders` si no existen todav\u00eda en `app.db`. Esto evita romper bases de datos SQLite ya creadas por instalaciones previas del demo.

---

## Manejo de Errores

### Estrategia de Manejo de Errores

La aplicación implementa múltiples capas de manejo de errores:

#### 1. Validación de Entrada

```csharp
public async Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail, CustomerAddressInput address, CancellationToken ct = default)
{
    ArgumentNullException.ThrowIfNull(address);

    var product = await _products.GetByIdAsync(productId, ct);
    if (product is null) 
        throw new KeyNotFoundException("Producto no encontrado");

    // Continuar con la lógica...
}
```

#### 2. Errores de Integración con Niubiz

```csharp
public async Task<string> GetSecurityTokenAsync(CancellationToken ct = default)
{
    try
    {
        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"No se pudo obtener el security token (HTTP {(int)res.StatusCode}): {body}");

        return body.Trim().Trim('"');
    }
    catch (HttpRequestException ex)
    {
        throw new InvalidOperationException("Error de conectividad con Niubiz", ex);
    }
}
```

#### 3. Errores en Controladores

```csharp
[HttpPost("/checkout/pay/{productId:guid}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Pay(Guid productId, CheckoutPayViewModel model, CancellationToken ct = default)
{
    var product = await _products.GetByIdAsync(productId, ct);
    if (product is null)
    {
        TempData["PaymentMessage"] = "Producto no encontrado. Refresca la lista e inténtalo otra vez.";
        return RedirectToAction("Index", "Products");
    }

    if (!ModelState.IsValid)
    {
        return View("Pay", model); // Redibuja el formulario con los mensajes de validación
    }

    try
    {
        var address = new CustomerAddressInput(model.Address, model.City, model.State, model.Country, model.PostalCode, model.Phone);
        var init = await _checkout.InitAsync(productId, model.CustomerEmail, address, ct);

        model.SessionKey = init.SessionKey;
        model.MerchantId = init.MerchantId;
        model.PurchaseNumber = init.PurchaseNumber;
        model.StaticJsUrl = init.StaticJsUrl;
        return View("Pay", model); // Ahora HasSession = true y se renderiza el widget de Niubiz
    }
    catch (KeyNotFoundException)
    {
        TempData["PaymentMessage"] = "Producto no encontrado. Refresca la lista e inténtalo otra vez.";
        return RedirectToAction("Index", "Products");
    }
}
```

### Códigos de Error Comunes

#### Niubiz API

| Código | Descripción | Acción Recomendada |
|--------|-------------|-------------------|
| `000` | Transacción aprobada | Continuar con el flujo |
| `100` | Transacción rechazada | Mostrar mensaje al usuario |
| `101` | Tarjeta vencida | Solicitar otra tarjeta |
| `102` | Tarjeta restringida | Contactar al banco |
| `200` | Monto no disponible | Verificar límite de tarjeta |
| `201` | Fecha de expiración incorrecta | Verificar datos de tarjeta |
| `202` | Código de seguridad incorrecto | Verificar CVV |

#### Errores de Aplicación

```csharp
public class ErrorCodes
{
    public const string PRODUCT_NOT_FOUND = "PRODUCT_NOT_FOUND";
    public const string INVALID_AMOUNT = "INVALID_AMOUNT";
    public const string NIUBIZ_CONNECTION_ERROR = "NIUBIZ_CONNECTION_ERROR";
    public const string INVALID_TRANSACTION_TOKEN = "INVALID_TRANSACTION_TOKEN";
    public const string ORDER_ALREADY_PROCESSED = "ORDER_ALREADY_PROCESSED";
}
```

### Logging

La aplicación utiliza el sistema de logging de .NET:

```csharp
public class NiubizClient : INiubizGateway
{
    private readonly ILogger<NiubizClient> _logger;

    public async Task<string> GetSecurityTokenAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Solicitando security token a Niubiz");
        
        try
        {
            // Lógica...
            _logger.LogInformation("Security token obtenido exitosamente");
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener security token de Niubiz");
            throw;
        }
    }
}
```

---

## Seguridad

### Medidas de Seguridad Implementadas

#### 1. Protección de Credenciales

- **User Secrets**: Para desarrollo local
- **Variables de Entorno**: Para producción
- **Configuración Separada**: Credenciales fuera del código fuente

#### 2. HTTPS Obligatorio

```csharp
// En Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

#### 3. Validación Anti-Forgery

```html
<!-- En formularios -->
@Html.AntiForgeryToken()
```

```csharp
[HttpPost("/checkout/pay/{productId:guid}")]
[ValidateAntiForgeryToken] // Habilitado en el POST que inicia la sesión Niubiz
public async Task<IActionResult> Pay(Guid productId, CheckoutPayViewModel model, CancellationToken ct = default)
```

> **Nota**: `/checkout/confirm` recibe el `POST` generado por el widget de Niubiz (`checkout.js`), que no incluye el antiforgery token del formulario propio, por lo que `[ValidateAntiForgeryToken]` está deshabilitado en ese endpoint. Si se expone este flujo públicamente, valide `purchaseNumber`/`transactionToken` contra el estado persistido en `PaymentTransaction` para mitigar abuso.

#### 4. Cookies Seguras

```csharp
Response.Cookies.Append("LastPurchaseNumber", purchaseNumber, new CookieOptions
{
    HttpOnly = true,      // No accesible desde JavaScript
    Secure = true,        // Solo HTTPS
    SameSite = SameSiteMode.Lax,
    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
});
```

#### 5. Sanitización de Datos

```csharp
// Validación de entrada
if (string.IsNullOrWhiteSpace(purchaseNumber) || purchaseNumber.Length > 20)
{
    return BadRequest("Número de compra inválido");
}

// Escape de HTML en vistas
@Html.Raw(Html.Encode(Model.Message))
```

### Recomendaciones de Seguridad para Producción

#### 1. Configuración de Headers de Seguridad

```csharp
app.Use((context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    return next();
});
```

#### 2. Rate Limiting

```csharp
// Instalar: AspNetCoreRateLimit
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
```

#### 3. Monitoreo y Auditoría

```csharp
_logger.LogWarning("Intento de pago con orden ya procesada: {PurchaseNumber}", purchaseNumber);
_logger.LogInformation("Pago exitoso - Orden: {PurchaseNumber}, Auth: {AuthCode}", 
    order.PurchaseNumber, result.AuthorizationCode);
```

#### 4. Validación de SSL/TLS

- Verificar certificados SSL válidos
- Usar TLS 1.2 o superior
- Implementar Certificate Pinning si es crítico

#### 5. Backup y Recuperación

```bash
# Backup de base de datos
cp app.db app.db.backup.$(date +%Y%m%d_%H%M%S)

# Rotación de logs
logrotate -f /etc/logrotate.d/integracion-niubiz
```

---

## Información Adicional

### Contacto y Soporte

- **Desarrollador**: Enrique Incio
- **Email**: enrique.incio@gmail.com
- **Repositorio**: https://github.com/eincioch/IntegracionNiubizDemo

### Documentación de Niubiz

- **Portal Desarrolladores**: https://desarrolladores.niubiz.com.pe/
- **API Reference**: https://desarrolladores.niubiz.com.pe/api/
- **Soporte Técnico**: soporte@niubiz.com.pe

### Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Fork el repositorio
2. Crear una rama para su feature (`git checkout -b feature/AmazingFeature`)
3. Commit sus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

### Licencia

Este proyecto está bajo la licencia MIT. Ver el archivo `LICENSE` para más detalles.

### Changelog

#### v1.1.0
- Corrección del payload de autorización Niubiz (`dataMap.serviceLocation*`) que causaba rechazos (`ACTION_CODE 670`)
- Captura de dirección/ubicación del comprador en el checkout, persistida en `Order` y enviada a Niubiz (`dataMap.cardholder*`) en la creación de sesión
- Reverse geocoding opcional vía Google Maps API (botón "Usar mi ubicación") a través de `IGeocodingService`
- `SchemaUpgrader` para evolucionar el esquema SQLite sin migraciones formales de EF Core
- Corrección de validación cliente en el campo `Country` (`RegularExpression` en vez de `StringLength` sobre un `<select>`)

#### v1.0.0 (2024-01-XX)
- Implementación inicial de integración con Niubiz
- Clean Architecture con .NET 8.0
- Funcionalidades básicas de e-commerce
- Documentación completa

---

**¡Gracias por usar IntegracionNiubizDemo!**

Este proyecto demuestra las mejores prácticas para integrar con la API de Niubiz y puede servir como base para implementaciones reales en producción.