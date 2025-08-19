# 🚀 Guía de Desarrollo

## Configuración del Entorno de Desarrollo

### Prerrequisitos

1. **.NET 8.0 SDK** o superior
   ```bash
   dotnet --version  # Verificar versión
   ```

2. **Editor de Código** (recomendado)
   - Visual Studio 2022 (17.8+)
   - Visual Studio Code con extensión C#
   - JetBrains Rider

3. **Base de Datos**
   - SQLite (incluido, sin instalación adicional)

4. **Herramientas Opcionales**
   - Postman/Insomnia para testing de APIs
   - DB Browser for SQLite para inspeccionar base de datos
   - Git para control de versiones

### Configuración Inicial

#### 1. Clonar y Configurar el Proyecto

```bash
# Clonar repositorio
git clone https://github.com/eincioch/IntegracionNiubizDemo.git
cd IntegracionNiubizDemo

# Restaurar dependencias
dotnet restore

# Verificar que compila
dotnet build
```

#### 2. Configurar User Secrets

```bash
# Inicializar user secrets para el proyecto Web
dotnet user-secrets init --project IntegracionNiubizDemo.Web

# Configurar credenciales de Niubiz QA (ejemplo)
dotnet user-secrets set "Niubiz:MerchantId" "522591303" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Username" "integraciones.visanet@necomplus.com" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Password" "d5e7nk$M" --project IntegracionNiubizDemo.Web

# Verificar configuración
dotnet user-secrets list --project IntegracionNiubizDemo.Web
```

#### 3. Configurar appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "IntegracionNiubizDemo": "Debug"
    }
  },
  "Niubiz": {
    "Environment": "qa",
    "Currency": "PEN",
    "BaseUrl": "https://apitestenv.vnforapps.com",
    "StaticJsUrl": "https://static-content-qas.vnforapps.com/v2/js/checkout.js",
    "Endpoints": {
      "Security": "/api.security/v1/security",
      "Session": "/api.ecommerce/v2/ecommerce/token/session/{merchantId}",
      "Authorization": "/api.authorization/v3/authorization/ecommerce/{merchantId}"
    }
  },
  "ConnectionStrings": {
    "Default": "Data Source=app-dev.db"
  }
}
```

## Estructura del Proyecto

### Convenciones de Naming

```
📁 Proyectos: PascalCase
   ├── 📁 Carpetas: PascalCase
   │   ├── 📄 Clases: PascalCase (Product.cs)
   │   ├── 🔧 Interfaces: IPascalCase (IProductService.cs)
   │   ├── 📊 DTOs: PascalCase + sufijo (CheckoutInitResult.cs)
   │   └── ⚙️ Configuración: PascalCase + Options (NiubizOptions.cs)
```

### Organización de Archivos

```
📦 IntegracionNiubizDemo/
├── 🎯 Domain/
│   ├── Entities/           # Entidades del dominio
│   │   ├── Product.cs
│   │   ├── Order.cs
│   │   └── PaymentTransaction.cs
│   └── Enums/             # Enumeraciones
│       └── OrderStatus.cs
├── 🔄 Application/
│   ├── Abstractions/      # Interfaces
│   │   ├── IProductService.cs
│   │   ├── ICheckoutService.cs
│   │   └── INiubizGateway.cs
│   ├── Services/          # Implementaciones de servicios
│   │   ├── ProductService.cs
│   │   └── CheckoutService.cs
│   ├── Dtos/             # Data Transfer Objects
│   │   └── CheckoutDtos.cs
│   └── DependencyInjection.cs
├── 💾 Persistence/
│   ├── Data/             # Contexto y configuraciones
│   │   └── AppDbContext.cs
│   ├── Repositories/     # Implementaciones de repositorios
│   │   ├── ProductRepository.cs
│   │   ├── OrderRepository.cs
│   │   └── PaymentRepository.cs
│   └── Migrations/       # Migraciones de EF (auto-generadas)
├── 🔌 Infrastructure/
│   ├── Niubiz/          # Integración con Niubiz
│   │   ├── NiubizClient.cs
│   │   └── NiubizOptions.cs
│   └── DependencyInjection.cs
└── 🌐 Web/
    ├── Controllers/      # Controladores MVC
    │   ├── ProductsController.cs
    │   ├── CheckoutController.cs
    │   └── HomeController.cs
    ├── Views/           # Vistas Razor
    │   ├── Products/
    │   ├── Checkout/
    │   └── Shared/
    ├── Models/          # ViewModels
    ├── wwwroot/         # Recursos estáticos
    └── Program.cs       # Punto de entrada
```

## Comandos de Desarrollo

### Construcción y Ejecución

```bash
# Compilar todo
dotnet build

# Compilar en modo Release
dotnet build -c Release

# Ejecutar aplicación web
dotnet run --project IntegracionNiubizDemo.Web

# Ejecutar con perfil específico
dotnet run --project IntegracionNiubizDemo.Web --launch-profile "https"

# Watch mode (recarga automática)
dotnet watch run --project IntegracionNiubizDemo.Web
```

### Entity Framework

```bash
# Agregar migración
dotnet ef migrations add NombreMigracion --project IntegracionNiubizDemo.Persistence --startup-project IntegracionNiubizDemo.Web

# Aplicar migraciones
dotnet ef database update --project IntegracionNiubizDemo.Persistence --startup-project IntegracionNiubizDemo.Web

# Remover última migración
dotnet ef migrations remove --project IntegracionNiubizDemo.Persistence --startup-project IntegracionNiubizDemo.Web

# Ver migraciones pendientes
dotnet ef migrations list --project IntegracionNiubizDemo.Persistence --startup-project IntegracionNiubizDemo.Web
```

### Testing

```bash
# Ejecutar todos los tests (cuando existan)
dotnet test

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests específicos
dotnet test --filter "TestCategory=Integration"
```

### Limpieza

```bash
# Limpiar objetos compilados
dotnet clean

# Limpiar y restaurar
dotnet clean && dotnet restore && dotnet build
```

## Patrones de Desarrollo

### 1. Repository Pattern

#### Interfaz
```csharp
public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
}
```

#### Implementación
```csharp
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    
    public ProductRepository(AppDbContext context) 
        => _context = context;
    
    public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        => _context.Products.AsNoTracking().ToListAsync(ct);
    
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Products.AsNoTracking()
                  .FirstOrDefaultAsync(p => p.Id == id, ct);
    
    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);
    }
    
    // ... otros métodos
}
```

### 2. Service Pattern

```csharp
public class CheckoutService : ICheckoutService
{
    private readonly IProductRepository _productRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly INiubizGateway _niubizGateway;
    private readonly ILogger<CheckoutService> _logger;
    
    public CheckoutService(
        IProductRepository productRepo,
        IOrderRepository orderRepo,
        IPaymentRepository paymentRepo,
        INiubizGateway niubizGateway,
        ILogger<CheckoutService> logger)
    {
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _paymentRepo = paymentRepo;
        _niubizGateway = niubizGateway;
        _logger = logger;
    }
    
    public async Task<CheckoutInitResult> InitAsync(
        Guid productId, 
        string? customerEmail, 
        CancellationToken ct = default)
    {
        using var activity = _logger.BeginScope("CheckoutInit {ProductId}", productId);
        
        try
        {
            // Lógica del método...
            _logger.LogInformation("Checkout iniciado para producto {ProductId}", productId);
            // ...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error iniciando checkout para producto {ProductId}", productId);
            throw;
        }
    }
}
```

### 3. Options Pattern

```csharp
// Configuración
public class NiubizOptions
{
    public const string SectionName = "Niubiz";
    
    public string Environment { get; set; } = "qa";
    public string MerchantId { get; set; } = default!;
    // ... propiedades
}

// Registro en DI
services.Configure<NiubizOptions>(
    configuration.GetSection(NiubizOptions.SectionName));

// Uso en servicio
public class NiubizClient
{
    private readonly NiubizOptions _options;
    
    public NiubizClient(IOptions<NiubizOptions> options)
    {
        _options = options.Value;
    }
}
```

## Debugging y Logging

### Configuración de Logging

```csharp
// Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
```

### Uso de ILogger

```csharp
public class CheckoutService
{
    private readonly ILogger<CheckoutService> _logger;
    
    public async Task<CheckoutInitResult> InitAsync(...)
    {
        _logger.LogInformation("Iniciando checkout para producto {ProductId}", productId);
        
        try
        {
            // ...código...
            _logger.LogDebug("Token de seguridad obtenido: {TokenLength} caracteres", token.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en checkout para producto {ProductId}", productId);
            throw;
        }
    }
}
```

### Debugging con Visual Studio

1. **Breakpoints**: F9 para toggle
2. **Step Over**: F10
3. **Step Into**: F11
4. **Continue**: F5
5. **Watch Windows**: Variables, Locals, Call Stack

### Debugging con VS Code

```json
// launch.json
{
    "name": ".NET Core Launch (web)",
    "type": "coreclr",
    "request": "launch",
    "preLaunchTask": "build",
    "program": "${workspaceFolder}/IntegracionNiubizDemo.Web/bin/Debug/net8.0/IntegracionNiubizDemo.Web.dll",
    "args": [],
    "cwd": "${workspaceFolder}/IntegracionNiubizDemo.Web",
    "stopAtEntry": false,
    "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
    },
    "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
    }
}
```

## Testing Guidelines

### Unit Tests (Ejemplo con NUnit)

```csharp
[TestFixture]
public class ProductServiceTests
{
    private IProductService _service;
    private Mock<IProductRepository> _mockRepo;
    
    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IProductRepository>();
        _service = new ProductService(_mockRepo.Object);
    }
    
    [Test]
    public async Task GetProductsAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Product", Price = 100m }
        };
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(products);
        
        // Act
        var result = await _service.GetProductsAsync();
        
        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Test Product"));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class CheckoutIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configurar servicios de test
                    services.Configure<NiubizOptions>(opts =>
                    {
                        opts.Environment = "qa";
                        // ... configuración de test
                    });
                });
            });
        
        _client = _factory.CreateClient();
    }
    
    [Test]
    public async Task PaymentFlow_WithValidData_CompletesSuccessfully()
    {
        // Test de integración completo
    }
}
```

## Performance y Optimización

### Entity Framework Best Practices

```csharp
// ✅ Bueno: AsNoTracking para consultas de solo lectura
public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
    => _context.Products.AsNoTracking().ToListAsync(ct);

// ✅ Bueno: Projection para DTOs
public Task<List<ProductDto>> GetProductDtosAsync(CancellationToken ct = default)
    => _context.Products
        .Select(p => new ProductDto { Id = p.Id, Name = p.Name })
        .ToListAsync(ct);

// ❌ Malo: N+1 queries
public async Task<List<OrderWithPayments>> GetOrdersWithPayments()
{
    var orders = await _context.Orders.ToListAsync();
    foreach (var order in orders)
    {
        order.Payments = await _context.PaymentTransactions
            .Where(p => p.OrderId == order.Id)
            .ToListAsync(); // ❌ Query por cada orden
    }
    return orders;
}

// ✅ Bueno: Include para cargar datos relacionados
public Task<List<Order>> GetOrdersWithPaymentsAsync()
    => _context.Orders
        .Include(o => o.PaymentTransactions)
        .ToListAsync();
```

### HttpClient Best Practices

```csharp
// ✅ Configuración del HttpClient
services.AddHttpClient<INiubizGateway, NiubizClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "IntegracionNiubizDemo/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // Solo para QA
});

// ✅ Retry Policy (con Polly)
services.AddHttpClient<INiubizGateway, NiubizClient>()
    .AddPolicyHandler(GetRetryPolicy());

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .Or<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

## Git Workflow

### Convenciones de Commit

```bash
# Formato: tipo(scope): descripción
git commit -m "feat(checkout): agregar validación de monto"
git commit -m "fix(niubiz): corregir encoding de credenciales"
git commit -m "docs(readme): actualizar instrucciones de setup"
git commit -m "refactor(repos): simplificar queries de productos"

# Tipos comunes:
# feat: nueva funcionalidad
# fix: corrección de bug
# docs: cambios en documentación
# style: formateo, espacios, etc.
# refactor: cambio de código sin cambiar funcionalidad
# test: agregar o modificar tests
# chore: mantenimiento, dependencias, etc.
```

### Branching Strategy

```bash
# Feature branch
git checkout -b feature/checkout-improvements
git push -u origin feature/checkout-improvements

# Bug fix
git checkout -b fix/payment-validation
git push -u origin fix/payment-validation

# Release preparation
git checkout -b release/v1.1.0
git push -u origin release/v1.1.0
```

## Deployment

### Configuración de Producción

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "IntegracionNiubizDemo": "Information"
    }
  },
  "AllowedHosts": "tudominio.com",
  "ConnectionStrings": {
    "Default": "Data Source=/app/data/app.db"
  },
  "Niubiz": {
    "Environment": "prod",
    "BaseUrl": "https://api.vnforapps.com",
    "StaticJsUrl": "https://static-content.vnforapps.com/v2/js/checkout.js"
  }
}
```

### Variables de Entorno

```bash
# Producción - usar variables de entorno para credenciales
export Niubiz__MerchantId="TU_MERCHANT_ID"
export Niubiz__Username="TU_USERNAME" 
export Niubiz__Password="TU_PASSWORD"
export ConnectionStrings__Default="TU_CONNECTION_STRING"
```

### Docker (Opcional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IntegracionNiubizDemo.Web/IntegracionNiubizDemo.Web.csproj", "IntegracionNiubizDemo.Web/"]
COPY ["IntegracionNiubizDemo.Infrastructure/IntegracionNiubizDemo.Infrastructure.csproj", "IntegracionNiubizDemo.Infrastructure/"]
COPY ["IntegracionNiubizDemo.Application/IntegracionNiubizDemo.Application.csproj", "IntegracionNiubizDemo.Application/"]
COPY ["IntegracionNiubizDemo.Persistence/IntegracionNiubizDemo.Persistence.csproj", "IntegracionNiubizDemo.Persistence/"]
COPY ["IntegracionNiubizDemo.Domain/IntegracionNiubizDemo.Domain.csproj", "IntegracionNiubizDemo.Domain/"]

RUN dotnet restore "IntegracionNiubizDemo.Web/IntegracionNiubizDemo.Web.csproj"
COPY . .
WORKDIR "/src/IntegracionNiubizDemo.Web"
RUN dotnet build "IntegracionNiubizDemo.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IntegracionNiubizDemo.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntegracionNiubizDemo.Web.dll"]
```

## Recursos Adicionales

### Herramientas Útiles

- **EF Core CLI**: `dotnet tool install --global dotnet-ef`
- **User Secrets**: `dotnet user-secrets --help`
- **ASP.NET Code Generator**: `dotnet tool install --global dotnet-aspnet-codegenerator`

### Extensiones VS Code Recomendadas

- C# for Visual Studio Code
- NuGet Package Manager
- REST Client
- GitLens
- Thunder Client (para testing de APIs)

### Shortcuts Útiles

| Acción | VS Code | Visual Studio |
|--------|---------|---------------|
| Compilar | `Ctrl+Shift+P` → "build" | `Ctrl+Shift+B` |
| Debug | `F5` | `F5` |
| Ir a definición | `F12` | `F12` |
| Buscar símbolo | `Ctrl+T` | `Ctrl+T` |
| Refactoring | `Ctrl+.` | `Ctrl+.` |