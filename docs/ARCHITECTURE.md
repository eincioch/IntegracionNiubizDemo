# Arquitectura del Proyecto - IntegracionNiubizDemo

## Resumen Ejecutivo

Este documento describe la arquitectura técnica del proyecto **IntegracionNiubizDemo**, implementado siguiendo los principios de **Clean Architecture** para garantizar mantenibilidad, testabilidad y separación de responsabilidades.

## Principios de Diseño Aplicados

### 1. Clean Architecture (Arquitectura Limpia)

La aplicación está estructurada en capas concéntricas donde:
- Las capas internas no conocen las externas
- Las dependencias apuntan hacia el centro
- La lógica de negocio está aislada de frameworks y tecnologías

### 2. Dependency Inversion Principle

- Las capas de alto nivel no dependen de las de bajo nivel
- Ambas dependen de abstracciones (interfaces)
- Las abstracciones no dependen de detalles

### 3. Single Responsibility Principle

- Cada clase tiene una sola razón para cambiar
- Separación clara entre persistencia, lógica de negocio y presentación

### 4. Interface Segregation Principle

- Interfaces específicas y cohesivas
- Los clientes no dependen de interfaces que no usan

## Diagramas de Arquitectura

### Diagrama de Capas

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│                 (IntegracionNiubizDemo.Web)            │
│   ┌─────────────┬─────────────┬─────────────────────┐   │
│   │ Controllers │    Views    │     wwwroot         │   │
│   │             │             │   (Static Files)    │   │
│   └─────────────┴─────────────┴─────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                      │
│              (IntegracionNiubizDemo.Application)       │
│   ┌─────────────┬─────────────┬─────────────────────┐   │
│   │   Services  │ Abstractions│       DTOs          │   │
│   │             │ (Interfaces)│                     │   │
│   └─────────────┴─────────────┴─────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────┐
│                   Domain Layer                          │
│                (IntegracionNiubizDemo.Domain)          │
│   ┌─────────────────────────────────────────────────┐   │
│   │                  Entities                       │   │
│   │          (Business Objects)                     │   │
│   └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────┐
│                Infrastructure Layer                     │
│             (IntegracionNiubizDemo.Infrastructure)     │
│   ┌─────────────┬─────────────────────────────────────┐ │
│   │    Niubiz   │          External APIs             │ │
│   │ Integration │           & Services                │ │
│   └─────────────┴─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────┐
│                 Persistence Layer                       │
│              (IntegracionNiubizDemo.Persistence)       │
│   ┌─────────────┬─────────────────────────────────────┐ │
│   │  DbContext  │          Repositories               │ │
│   │             │         (Data Access)               │ │
│   └─────────────┴─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### Diagrama de Flujo de Dependencias

```
Web Layer
    │
    ├─► Application Layer ◄─── Infrastructure Layer
    │         │                        │
    │         ▼                        │
    │   Domain Layer ◄─────────────────┘
    │         ▲                        
    │         │                        
    └─────────┼─► Persistence Layer ───┘
              │         │
              └─────────┘
```

## Descripción Detallada de Capas

### 1. Domain Layer (Núcleo del Negocio)

**Responsabilidad**: Contiene la lógica de negocio pura y las entidades del dominio.

**Características**:
- No tiene dependencias externas
- Define las reglas de negocio fundamentales
- Contiene entidades, value objects y domain services

**Componentes**:
```
IntegracionNiubizDemo.Domain/
├── Entities/
│   ├── Product.cs          # Producto del catálogo
│   ├── Order.cs           # Orden de compra
│   └── PaymentTransaction.cs # Transacción de pago
```

**Ejemplo de Entidad**:
```csharp
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PurchaseNumber { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Métodos de dominio
    public void MarkAsPaid(string authorizationCode)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order is not in pending status");
            
        Status = OrderStatus.Paid;
        // Lógica adicional...
    }
}
```

### 2. Application Layer (Servicios de Aplicación)

**Responsabilidad**: Orquesta las operaciones de negocio y coordina entre el dominio y la infraestructura.

**Características**:
- Define casos de uso de la aplicación
- Contiene interfaces para la infraestructura
- Maneja la coordinación entre entidades

**Componentes**:
```
IntegracionNiubizDemo.Application/
├── Services/
│   ├── ProductService.cs      # Gestión de productos
│   └── CheckoutService.cs     # Proceso de checkout
├── Abstractions/
│   ├── IProductService.cs     # Interface de productos
│   ├── ICheckoutService.cs    # Interface de checkout
│   ├── INiubizGateway.cs      # Interface para Niubiz
│   └── I*Repository.cs        # Interfaces de repositorios
├── Dtos/
│   └── CheckoutDtos.cs        # Objetos de transferencia
└── DependencyInjection.cs    # Configuración DI
```

**Ejemplo de Servicio**:
```csharp
public class CheckoutService : ICheckoutService
{
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly INiubizGateway _niubiz;

    public async Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail)
    {
        // 1. Validar producto
        var product = await _products.GetByIdAsync(productId);
        if (product is null) throw new KeyNotFoundException("Producto no encontrado");

        // 2. Crear orden
        var order = new Order { /* ... */ };
        await _orders.AddAsync(order);

        // 3. Integrar con Niubiz
        var securityToken = await _niubiz.GetSecurityTokenAsync();
        var sessionKey = await _niubiz.CreateSessionAsync(securityToken, order.Amount, order.PurchaseNumber, order.Currency);

        return new CheckoutInitResult(/* ... */);
    }
}
```

### 3. Infrastructure Layer (Infraestructura)

**Responsabilidad**: Implementa las interfaces definidas en la capa de aplicación para interactuar con sistemas externos.

**Características**:
- Implementa gateways para APIs externas
- Configura servicios de infraestructura
- Maneja aspectos técnicos (HTTP, serialización, etc.)

**Componentes**:
```
IntegracionNiubizDemo.Infrastructure/
├── Niubiz/
│   ├── NiubizClient.cs        # Cliente HTTP para Niubiz API
│   └── NiubizOptions.cs       # Configuración de Niubiz
└── DependencyInjection.cs    # Registro de servicios
```

**Ejemplo de Gateway**:
```csharp
public class NiubizClient : INiubizGateway
{
    private readonly HttpClient _http;
    private readonly NiubizOptions _options;

    public async Task<string> GetSecurityTokenAsync(CancellationToken ct = default)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.SecurityEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
```

### 4. Persistence Layer (Persistencia)

**Responsabilidad**: Maneja el acceso a datos y la persistencia de entidades.

**Características**:
- Implementa el patrón Repository
- Configura Entity Framework
- Abstrae el acceso a la base de datos

**Componentes**:
```
IntegracionNiubizDemo.Persistence/
├── Data/
│   └── AppDbContext.cs        # Contexto de Entity Framework
└── Repositories/
    ├── ProductRepository.cs   # Repositorio de productos
    ├── OrderRepository.cs     # Repositorio de órdenes
    └── PaymentRepository.cs   # Repositorio de pagos
```

**Ejemplo de Repositorio**:
```csharp
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Products.ToListAsync(ct);
    }
}
```

### 5. Presentation Layer (Presentación)

**Responsabilidad**: Maneja la interfaz de usuario y la interacción con el usuario final.

**Características**:
- Controladores MVC
- Vistas Razor
- APIs REST
- Validación de entrada

**Componentes**:
```
IntegracionNiubizDemo.Web/
├── Controllers/
│   ├── ProductsController.cs  # Gestión de productos
│   ├── CheckoutController.cs  # Proceso de pago
│   └── HomeController.cs      # Página principal
├── Views/
│   ├── Products/              # Vistas de productos
│   ├── Checkout/              # Vistas de checkout
│   └── Shared/                # Vistas compartidas
├── wwwroot/                   # Archivos estáticos
└── Program.cs                 # Configuración de la aplicación
```

## Patrones de Diseño Implementados

### 1. Repository Pattern

**Propósito**: Encapsular la lógica de acceso a datos y proporcionar una interfaz más orientada a objetos.

**Implementación**:
```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
}
```

### 2. Gateway Pattern

**Propósito**: Encapsular el acceso a sistemas externos (API de Niubiz).

**Implementación**:
```csharp
public interface INiubizGateway
{
    Task<string> GetSecurityTokenAsync(CancellationToken ct = default);
    Task<string> CreateSessionAsync(string securityToken, decimal amount, string purchaseNumber, string currency, CancellationToken ct = default);
    Task<AuthorizationResult> AuthorizeAsync(string securityToken, string transactionToken, decimal amount, string currency, string purchaseNumber, CancellationToken ct = default);
}
```

### 3. Dependency Injection

**Propósito**: Invertir el control de dependencias para mejorar testabilidad y flexibilidad.

**Implementación**:
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        return services;
    }
}
```

### 4. Options Pattern

**Propósito**: Configurar servicios de manera tipada y validada.

**Implementación**:
```csharp
public class NiubizOptions
{
    public string MerchantId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Currency { get; set; } = "PEN";
}

// Configuración
services.Configure<NiubizOptions>(configuration.GetSection("Niubiz"));
```

## Flujo de Datos

### Flujo de Lectura (Query)

```
Controller → Service → Repository → Database
     ↓
   View ← DTO ← Entity ← Entity
```

### Flujo de Escritura (Command)

```
Controller → Service → Gateway/Repository → External API/Database
     ↓              ↓
   View ← Result ← Domain Logic
```

### Ejemplo Completo: Proceso de Checkout

```
1. CheckoutController.Pay()
   ↓
2. CheckoutService.InitAsync()
   ├─► ProductRepository.GetByIdAsync() → Database
   ├─► OrderRepository.AddAsync() → Database  
   └─► NiubizGateway.GetSecurityTokenAsync() → Niubiz API
       └─► NiubizGateway.CreateSessionAsync() → Niubiz API
   ↓
3. CheckoutInitResult → View
```

## Beneficios de esta Arquitectura

### 1. Mantenibilidad
- Separación clara de responsabilidades
- Código fácil de entender y modificar
- Cambios aislados en capas específicas

### 2. Testabilidad
- Interfaces permiten mocking fácil
- Lógica de negocio aislada
- Tests unitarios e integración independientes

### 3. Flexibilidad
- Fácil cambio de proveedores de pago
- Intercambio de base de datos sin afectar lógica
- Evolución independiente de capas

### 4. Escalabilidad
- Separación permite scaling horizontal
- Microservicios como evolución natural
- Optimización por capa

## Consideraciones para Producción

### 1. Performance
- Implementar caché en servicios
- Optimizar consultas de base de datos
- Connection pooling para HTTP clients

### 2. Monitoreo
- Logging estructurado por capa
- Métricas de performance
- Health checks por componente

### 3. Seguridad
- Validación en múltiples capas
- Sanitización de datos
- Manejo seguro de credenciales

### 4. Resilencia
- Circuit breaker para APIs externas
- Retry policies con backoff
- Timeouts apropiados

## Conclusión

La arquitectura implementada en **IntegracionNiubizDemo** proporciona una base sólida para aplicaciones empresariales, balanceando simplicidad con escalabilidad y mantenibilidad. Los principios de Clean Architecture garantizan que el código sea testeable, flexible y resistente a cambios tecnológicos.