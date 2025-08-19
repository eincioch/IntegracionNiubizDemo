# 🏗️ Arquitectura del Sistema

## Visión General

IntegracionNiubizDemo implementa **Clean Architecture** (Arquitectura Limpia) propuesta por Robert C. Martin, asegurando:

- 🎯 **Separación de responsabilidades**
- 🔧 **Facilidad de mantenimiento**
- 🧪 **Testabilidad**
- 🔌 **Flexibilidad para cambios**

## Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    🌐 PRESENTATION LAYER                    │
│                   (IntegracionNiubizDemo.Web)               │
├─────────────────────┬─────────────────────┬─────────────────┤
│   ProductsController │   CheckoutController │   HomeController │
│                     │                     │                 │
│   - Catálogo        │   - Iniciar pago    │   - Dashboard   │
│   - Listado         │   - Confirmar pago  │   - Navegación  │
└─────────────────────┼─────────────────────┼─────────────────┘
                      │                     │
                      ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  🔄 APPLICATION LAYER                       │
│                (IntegracionNiubizDemo.Application)          │
├─────────────────────┬─────────────────────┬─────────────────┤
│   IProductService   │   ICheckoutService  │   Abstracciones │
│                     │                     │                 │
│   - Gestión         │   - Flujo de pago   │   - INiubizGateway
│   - Seed data       │   - Validaciones    │   - IRepositorios
└─────────────────────┼─────────────────────┼─────────────────┘
                      │                     │
                      ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  🔌 INFRASTRUCTURE LAYER                    │
│              (IntegracionNiubizDemo.Infrastructure)         │
├─────────────────────┬─────────────────────┬─────────────────┤
│   NiubizClient      │   HttpClient        │   DI Container  │
│                     │                     │                 │
│   - API Integration │   - HTTP requests   │   - Inyección   │
│   - Token management│   - SSL/Security    │   - Config.     │
└─────────────────────┼─────────────────────┼─────────────────┘
                      │                     │
                      ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│                   💾 PERSISTENCE LAYER                      │
│               (IntegracionNiubizDemo.Persistence)           │
├─────────────────────┬─────────────────────┬─────────────────┤
│   AppDbContext      │   Repositories      │   Migrations    │
│                     │                     │                 │
│   - EF Core Setup   │   - CRUD Operations │   - DB Schema   │
│   - Entity Config   │   - Query Logic     │   - Versioning  │
└─────────────────────┼─────────────────────┼─────────────────┘
                      │                     │
                      ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    🎯 DOMAIN LAYER                          │
│                 (IntegracionNiubizDemo.Domain)              │
├─────────────────────┬─────────────────────┬─────────────────┤
│   Product           │   Order             │   PaymentTx     │
│                     │                     │                 │
│   - Business Entity │   - Order Logic     │   - Tx Tracking │
│   - Rules & Logic   │   - Status enum     │   - Status      │
└─────────────────────┴─────────────────────┴─────────────────┘
```

## Capas Detalladas

### 🎯 Domain Layer (Núcleo del Negocio)

**Responsabilidades:**
- Definir entidades del negocio
- Establecer reglas de negocio
- No tiene dependencias externas

**Entidades Principales:**
```csharp
// Product: Representa productos del catálogo
public class Product 
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Order: Representa órdenes de compra
public class Order 
{
    public Guid Id { get; set; }
    public string PurchaseNumber { get; set; }
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    // ...
}

// PaymentTransaction: Rastrea transacciones de pago
public class PaymentTransaction 
{
    public Guid Id { get; set; }
    public string? SessionKey { get; set; }
    public string? TransactionToken { get; set; }
    public string Status { get; set; }
    // ...
}
```

### 🔄 Application Layer (Casos de Uso)

**Responsabilidades:**
- Definir casos de uso de la aplicación
- Coordinar flujos de trabajo
- Definir abstracciones para dependencias externas

**Servicios Principales:**

#### IProductService
```csharp
public interface IProductService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default);
    Task EnsureSeedDataAsync(CancellationToken ct = default);
}
```

#### ICheckoutService
```csharp
public interface ICheckoutService
{
    Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail, CancellationToken ct = default);
    Task<ConfirmResult> ConfirmAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default);
}
```

#### INiubizGateway
```csharp
public interface INiubizGateway
{
    Task<string> GetSecurityTokenAsync(CancellationToken ct = default);
    Task<string> CreateSessionAsync(string securityToken, decimal amount, string purchaseNumber, string currency, CancellationToken ct = default);
    Task<AuthorizationResult> AuthorizeAsync(string securityToken, string transactionToken, decimal amount, string currency, string purchaseNumber, CancellationToken ct = default);
}
```

### 💾 Persistence Layer (Acceso a Datos)

**Responsabilidades:**
- Implementar repositorios
- Configurar Entity Framework
- Manejar persistencia de datos

**Componentes:**
- `AppDbContext`: Contexto de Entity Framework
- `ProductRepository`: CRUD para productos
- `OrderRepository`: CRUD para órdenes
- `PaymentRepository`: CRUD para transacciones

### 🔌 Infrastructure Layer (Integraciones)

**Responsabilidades:**
- Implementar integraciones externas
- Configurar clientes HTTP
- Manejar infraestructura técnica

**Componentes Clave:**

#### NiubizClient
```csharp
public class NiubizClient : INiubizGateway
{
    // Implementa la integración completa con Niubiz
    // - Autenticación con Basic Auth
    // - Manejo de tokens de seguridad
    // - Procesamiento de pagos
    // - Manejo de errores
}
```

#### NiubizOptions
```csharp
public class NiubizOptions
{
    public string Environment { get; set; } = "qa";
    public string MerchantId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    // ... endpoints configurables
}
```

### 🌐 Web Layer (Presentación)

**Responsabilidades:**
- Manejar requests HTTP
- Presentar datos al usuario
- Validar entrada del usuario

**Controladores:**

#### ProductsController
- Lista productos disponibles
- Página principal del catálogo

#### CheckoutController
- Inicia proceso de pago
- Confirma transacciones
- Maneja callbacks de Niubiz

## Flujo de Datos

### 1. Listado de Productos
```
Usuario → ProductsController → IProductService → IProductRepository → Database
     ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ←
```

### 2. Proceso de Pago
```
Usuario → CheckoutController → ICheckoutService → INiubizGateway → Niubiz API
     ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ←
```

### 3. Persistencia de Transacción
```
ICheckoutService → IPaymentRepository → AppDbContext → SQLite Database
              ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ←
```

## Principios Aplicados

### 🎯 Dependency Inversion Principle
- Las capas internas no dependen de las externas
- Las abstracciones no dependen de detalles
- Los detalles dependen de abstracciones

### 🔧 Single Responsibility Principle
- Cada clase tiene una sola razón para cambiar
- Separación clara de responsabilidades

### 🔓 Open/Closed Principle
- Abierto para extensión, cerrado para modificación
- Fácil agregar nuevos gateways de pago

### 🔄 Interface Segregation Principle
- Interfaces específicas y cohesivas
- Clientes no dependen de métodos que no usan

## Ventajas de esta Arquitectura

### ✅ Testabilidad
- Fácil crear unit tests
- Mocking de dependencias externas
- Aislamiento de lógica de negocio

### ✅ Mantenibilidad
- Código organizado y modular
- Fácil localizar y corregir errores
- Refactoring seguro

### ✅ Flexibilidad
- Intercambiar implementaciones fácilmente
- Agregar nuevos providers de pago
- Cambiar base de datos sin afectar lógica

### ✅ Escalabilidad
- Separación clara permite escalar componentes
- Fácil distribución en microservicios
- Caching y optimizaciones focalizadas

## Consideraciones de Diseño

### 🔐 Seguridad
- Credenciales en User Secrets
- Validación de entrada
- Tokens temporales

### 📊 Observabilidad
- Logging estructurado
- Tracking de transacciones
- Manejo de errores

### ⚡ Performance
- Uso de CancellationToken
- Queries optimizadas
- HTTP client reutilizable