# 🤝 Contributing to IntegracionNiubizDemo

¡Gracias por tu interés en contribuir a **IntegracionNiubizDemo**! Este documento te guiará sobre cómo contribuir efectivamente al proyecto.

## 🎯 Formas de Contribuir

### 💻 Contribuciones de Código
- Implementar nuevas funcionalidades
- Corregir bugs reportados
- Mejorar el rendimiento
- Refactorizar código existente
- Agregar tests unitarios

### 📚 Contribuciones de Documentación
- Mejorar la documentación existente
- Agregar nuevos ejemplos
- Traducir documentación
- Corregir errores tipográficos
- Crear tutoriales

### 🐛 Reportes de Bugs
- Reportar problemas encontrados
- Verificar y reproducir bugs existentes
- Proporcionar información adicional en issues

### 💡 Sugerencias y Ideas
- Proponer nuevas funcionalidades
- Sugerir mejoras en la UX/UI
- Compartir casos de uso reales
- Proponer optimizaciones

## 🚀 Comenzando

### 1. Fork y Clone

```bash
# Fork el repositorio en GitHub, luego clona tu fork
git clone https://github.com/TU_USERNAME/IntegracionNiubizDemo.git
cd IntegracionNiubizDemo

# Agregar el upstream remote
git remote add upstream https://github.com/eincioch/IntegracionNiubizDemo.git
```

### 2. Configurar el Entorno

```bash
# Verificar .NET SDK
dotnet --version  # Debe ser 8.0+

# Restaurar dependencias
dotnet restore

# Configurar user secrets (opcional para desarrollo)
dotnet user-secrets init --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:MerchantId" "522591303" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Username" "integraciones.visanet@necomplus.com" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Password" "d5e7nk$M" --project IntegracionNiubizDemo.Web

# Verificar que compila
dotnet build

# Ejecutar la aplicación
dotnet run --project IntegracionNiubizDemo.Web
```

### 3. Crear una Rama

```bash
# Actualizar main
git checkout main
git pull upstream main

# Crear rama para tu feature/fix
git checkout -b feature/nueva-funcionalidad
# o
git checkout -b fix/corregir-bug
# o
git checkout -b docs/mejorar-documentacion
```

## 📋 Proceso de Desarrollo

### Convenciones de Naming

#### Ramas
```bash
# Features
feature/checkout-improvements
feature/multi-currency-support
feature/webhook-integration

# Bug fixes
fix/payment-validation-bug
fix/database-connection-issue
fix/ui-responsive-design

# Documentación
docs/api-examples
docs/troubleshooting-guide
docs/architecture-diagrams

# Refactoring
refactor/service-layer-cleanup
refactor/repository-pattern-improvements

# Tests
test/integration-tests
test/unit-test-coverage
```

#### Commits
Seguimos [Conventional Commits](https://www.conventionalcommits.org/):

```bash
# Formato: tipo(scope): descripción

# Ejemplos:
git commit -m "feat(checkout): agregar validación de monto máximo"
git commit -m "fix(niubiz): corregir encoding de credenciales especiales"
git commit -m "docs(readme): actualizar instrucciones de instalación"
git commit -m "refactor(repos): simplificar queries con async/await"
git commit -m "test(checkout): agregar tests de integración"
git commit -m "style(formatting): aplicar estilo consistente"
git commit -m "perf(database): optimizar consultas de productos"
```

**Tipos de commit**:
- `feat`: Nueva funcionalidad
- `fix`: Corrección de bug
- `docs`: Cambios en documentación
- `style`: Formateo, espacios, etc. (no cambios de código)
- `refactor`: Cambio de código sin cambiar funcionalidad
- `perf`: Mejoras de rendimiento
- `test`: Agregar o modificar tests
- `chore`: Mantenimiento, dependencias, etc.

### Estándares de Código

#### C# Coding Standards

```csharp
// ✅ Bueno: PascalCase para públicos
public class ProductService : IProductService
{
    // ✅ Bueno: camelCase para privados
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;
    
    // ✅ Bueno: Constructor con validación
    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // ✅ Bueno: Async methods con CancellationToken
    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Obteniendo lista de productos");
        
        try
        {
            return await _repository.GetAllAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo productos");
            throw;
        }
    }
}

// ❌ Malo: Sincrónico sin logging
public List<Product> GetProducts()
{
    return _repository.GetAll();
}
```

#### Principios a Seguir

1. **SOLID Principles**
   - Single Responsibility: Una clase, una responsabilidad
   - Open/Closed: Abierto para extensión, cerrado para modificación
   - Liskov Substitution: Las subclases deben ser intercambiables
   - Interface Segregation: Interfaces específicas y cohesivas
   - Dependency Inversion: Depender de abstracciones, no de concretos

2. **Clean Code**
   - Nombres descriptivos para variables y métodos
   - Métodos pequeños y enfocados
   - Comentarios solo cuando son necesarios
   - Manejo consistente de errores

3. **Async/Await Best Practices**
   - Usar `async`/`await` para operaciones I/O
   - Pasar `CancellationToken` en métodos async
   - No usar `.Result` o `.Wait()`
   - Configurar `ConfigureAwait(false)` en librerías

### Testing Guidelines

#### Unit Tests
```csharp
[TestFixture]
public class ProductServiceTests
{
    private ProductService _service;
    private Mock<IProductRepository> _mockRepository;
    private Mock<ILogger<ProductService>> _mockLogger;
    
    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Test]
    public async Task GetProductsAsync_WhenRepositoryReturnsProducts_ReturnsProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Product", Price = 100m }
        };
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _service.GetProductsAsync();
        
        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Test Product"));
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public void GetProductsAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));
        
        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetProductsAsync());
    }
}
```

#### Integration Tests
```csharp
[TestFixture]
public class CheckoutIntegrationTests : IDisposable
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Configurar servicios de test
                    services.Configure<NiubizOptions>(opts =>
                    {
                        opts.Environment = "qa";
                        opts.MerchantId = "TEST_MERCHANT";
                        // etc.
                    });
                });
            });
        
        _client = _factory.CreateClient();
    }
    
    [Test]
    public async Task GET_Products_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/Products");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.That(response.Content.Headers.ContentType?.ToString(), 
                   Does.StartWith("text/html"));
    }
    
    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
```

## 📝 Pull Request Process

### 1. Antes de Enviar

```bash
# Asegurarte de que todo funciona
dotnet build
dotnet test  # Si hay tests

# Verificar formato de código
dotnet format

# Actualizar con los últimos cambios
git fetch upstream
git rebase upstream/main

# Verificar commits
git log --oneline -5
```

### 2. Descripción del PR

**Template para Pull Requests**:

```markdown
## 📋 Descripción

Breve descripción de los cambios realizados.

## 🔄 Tipo de Cambio

- [ ] Bug fix (cambio que corrige un issue)
- [ ] Nueva funcionalidad (cambio que agrega funcionalidad)
- [ ] Breaking change (fix o feature que cambiaría funcionalidad existente)
- [ ] Documentación (cambios solo en documentación)

## 🧪 Testing

Describe las pruebas que ejecutaste para verificar tus cambios:

- [ ] Unit tests existentes pasan
- [ ] Agregué nuevos tests que prueban mi fix/feature
- [ ] Probé localmente la funcionalidad

## 📸 Screenshots (si aplica)

Si hay cambios visuales, incluye screenshots del antes y después.

## ✅ Checklist

- [ ] Mi código sigue las guías de estilo del proyecto
- [ ] He realizado self-review de mi código
- [ ] He comentado mi código en áreas complejas
- [ ] He actualizado la documentación correspondiente
- [ ] Mis cambios no generan nuevos warnings
- [ ] He agregado tests que prueban que mi fix/feature funciona
- [ ] Tests nuevos y existentes pasan localmente
```

### 3. Review Process

1. **Automated Checks**: CI/CD ejecutará builds y tests
2. **Code Review**: Un maintainer revisará tu código
3. **Feedback**: Podrían solicitar cambios
4. **Approval**: Una vez aprobado, se hará merge

## 🚫 Qué NO Hacer

### Cambios que NO Aceptamos

- ❌ **Breaking changes** sin discusión previa
- ❌ **Cambios masivos** de formatting sin funcionalidad
- ❌ **Dependencias innecesarias** o no mantenidas
- ❌ **Código no testeado** para funcionalidades críticas
- ❌ **Commits con credenciales** o información sensible
- ❌ **Changes que violan principios de Clean Architecture**

### Anti-patterns

```csharp
// ❌ NO: Hardcodear conexiones
public class BadService
{
    public void DoSomething()
    {
        var connection = "Data Source=localhost;..."; // ❌ Hardcoded
        // usar directamente
    }
}

// ❌ NO: Ignorar async/await
public List<Product> GetProducts() // ❌ Sincrónico
{
    return _repository.GetAllAsync().Result; // ❌ .Result bloquea
}

// ❌ NO: Excepciones genéricas
public void ProcessPayment()
{
    try
    {
        // código
    }
    catch (Exception) // ❌ Muy genérico
    {
        // manejo genérico
    }
}
```

## 🏷️ Issue Management

### Reportar Bugs

```markdown
**Describe el bug**
Descripción clara y concisa del problema.

**Para Reproducir**
Pasos para reproducir:
1. Ir a '...'
2. Hacer click en '....'
3. Scroll down hasta '....'
4. Ver error

**Comportamiento Esperado**
Descripción clara de lo que esperabas que pasara.

**Screenshots**
Si aplica, agregar screenshots del problema.

**Información del Entorno:**
 - OS: [ej. Windows 10, Ubuntu 20.04]
 - .NET Version: [ej. 8.0.7]
 - Browser: [ej. Chrome 96, Firefox 94]

**Información Adicional**
Cualquier contexto adicional sobre el problema.
```

### Proponer Features

```markdown
**¿Tu feature request está relacionado con un problema?**
Descripción clara del problema. Ej. "Siempre me frustra cuando [...]"

**Describe la solución que te gustaría**
Descripción clara y concisa de lo que quieres que pase.

**Describe alternativas que has considerado**
Descripción clara de cualquier solución o feature alternativa.

**Contexto adicional**
Cualquier otro contexto o screenshots sobre el feature request.
```

## 🎖️ Reconocimiento

### Contributors

Mantenemos una lista de contribuidores en:
- README principal
- Releases notes
- Contributors page (próximamente)

### Tipos de Contribución

- 💻 **Code**: Contribuciones de código
- 📖 **Documentation**: Mejoras en documentación  
- 🐛 **Bug reports**: Reportes de bugs valiosos
- 💡 **Ideas**: Sugerencias implementadas
- 🔍 **Reviews**: Code reviews útiles
- 📋 **Project Management**: Organización y planning

## 📞 Obtener Ayuda

### ¿Necesitas ayuda?

- 💬 **GitHub Discussions**: Para preguntas generales
- 📧 **Issues**: Para problemas específicos
- 📖 **Documentación**: Revisa la carpeta `docs/`

### ¿Primera contribución?

Si es tu primera contribución a open source:

1. Lee [First Contributions](https://github.com/firstcontributions/first-contributions)
2. Empieza con issues etiquetados como `good first issue`
3. No dudes en preguntar en Discussions

---

## 📜 Code of Conduct

Al participar en este proyecto, aceptas cumplir con nuestras normas de conducta:

### Nuestro Compromiso

Nos comprometemos a hacer de la participación en nuestro proyecto una experiencia libre de acoso para todos, independientemente de edad, tamaño corporal, discapacidad, etnia, identidad y expresión de género, nivel de experiencia, nacionalidad, apariencia personal, raza, religión, o identidad y orientación sexual.

### Nuestros Estándares

**Comportamientos que contribuyen a crear un ambiente positivo**:
- ✅ Usar lenguaje acogedor e inclusivo
- ✅ Respetar diferentes puntos de vista y experiencias
- ✅ Aceptar graciosamente críticas constructivas
- ✅ Enfocarse en lo que es mejor para la comunidad
- ✅ Mostrar empatía hacia otros miembros de la comunidad

**Comportamientos no aceptables**:
- ❌ Uso de lenguaje o imágenes sexualizadas
- ❌ Trolling, comentarios insultantes o ataques personales
- ❌ Acoso público o privado
- ❌ Publicar información privada sin permiso explícito
- ❌ Otras conductas que se consideren inapropiadas en un entorno profesional

---

¡Gracias por contribuir a **IntegracionNiubizDemo**! 🎉