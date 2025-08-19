# 🚀 IntegracionNiubizDemo

## Descripción General

**IntegracionNiubizDemo** es una aplicación de demostración que muestra cómo integrar el sistema de pagos **Niubiz** (anteriormente VisaNet) de forma profesional utilizando **Clean Architecture** en **.NET 8.0**. La aplicación presenta un catálogo de productos con funcionalidad de checkout y procesamiento de pagos seguro.

## 🎯 Características Principales

- ✅ **Arquitectura Limpia** (Clean Architecture) con separación clara de responsabilidades
- ✅ **Integración completa con Niubiz** para procesamiento de pagos
- ✅ **Catálogo de productos** con gestión de inventario
- ✅ **Checkout seguro** con tokens de transacción
- ✅ **Entity Framework Core** para persistencia de datos
- ✅ **Manejo robusto de errores** y logging
- ✅ **Configuración flexible** mediante appsettings y user secrets

## 🏗️ Arquitectura

El proyecto implementa Clean Architecture con las siguientes capas:

```
📦 IntegracionNiubizDemo
├── 🎯 Domain/              # Entidades del negocio y reglas centrales
│   ├── Entities/
│   │   ├── Product.cs      # Entidad Producto
│   │   ├── Order.cs        # Entidad Orden
│   │   └── PaymentTransaction.cs # Entidad Transacción de Pago
├── 🔄 Application/         # Casos de uso y abstracciones
│   ├── Abstractions/       # Interfaces y contratos
│   ├── Services/           # Servicios de aplicación
│   └── Dtos/              # Objetos de transferencia de datos
├── 💾 Persistence/         # Acceso a datos y repositorios
│   ├── Data/              # DbContext y configuraciones
│   └── Repositories/      # Implementaciones de repositorios
├── 🔌 Infrastructure/      # Integraciones externas
│   ├── Niubiz/            # Cliente e integración con Niubiz
│   └── DependencyInjection.cs
└── 🌐 Web/               # Capa de presentación ASP.NET Core MVC
    ├── Controllers/       # Controladores MVC
    ├── Views/            # Vistas Razor
    └── wwwroot/          # Recursos estáticos
```

## 🔌 Integración Niubiz

### Flujo de Pago

1. **Inicialización**: Se genera una sesión segura con Niubiz
2. **Presentación**: Se muestra el formulario de pago con token de sesión
3. **Procesamiento**: Niubiz procesa la transacción
4. **Confirmación**: Se valida y confirma el resultado del pago

### Endpoints Niubiz Utilizados

- **Security Token**: `/api.security/v1/security` - Autenticación
- **Session Token**: `/api.ecommerce/v2/ecommerce/token/session/{merchantId}` - Sesión de pago
- **Authorization**: `/api.authorization/v3/authorization/ecommerce/{merchantId}` - Autorización

## 🚀 Inicio Rápido

### Prerrequisitos

- **.NET 8.0 SDK** o superior
- **SQLite** (base de datos embebida)
- **Credenciales de Niubiz** (merchantId, username, password)

### 1. Clonar el Repositorio

```bash
git clone https://github.com/eincioch/IntegracionNiubizDemo.git
cd IntegracionNiubizDemo
```

### 2. Configurar Credenciales

Configura las credenciales de Niubiz usando User Secrets:

```bash
# Inicializar user secrets
dotnet user-secrets init --project IntegracionNiubizDemo.Web

# Configurar credenciales (reemplaza con tus datos reales)
dotnet user-secrets set "Niubiz:MerchantId" "TU_MERCHANT_ID" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Username" "TU_USERNAME" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Password" "TU_PASSWORD" --project IntegracionNiubizDemo.Web
```

### 3. Restaurar y Compilar

```bash
dotnet restore
dotnet build
```

### 4. Ejecutar la Aplicación

```bash
dotnet run --project IntegracionNiubizDemo.Web
```

La aplicación estará disponible en `https://localhost:5001` o `http://localhost:5000`.

## ⚙️ Configuración

### Configuración de Niubiz

En `appsettings.json` o `appsettings.Development.json`:

```json
{
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
  }
}
```

### Base de Datos

La aplicación utiliza SQLite por defecto. La cadena de conexión se puede configurar en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=app.db"
  }
}
```

## 🛠️ Desarrollo

### Estructura de Datos

#### Product (Producto)
```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

#### Order (Orden)
```csharp
public class Order
{
    public Guid Id { get; set; }
    public string PurchaseNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public string? CustomerEmail { get; set; }
}
```

#### PaymentTransaction (Transacción de Pago)
```csharp
public class PaymentTransaction
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string? SessionKey { get; set; }
    public string? TransactionToken { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? MaskedCard { get; set; }
    public string Status { get; set; }
    public string? RawResponse { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Servicios Principales

#### ICheckoutService
Maneja el flujo completo del checkout:
- `InitAsync()` - Inicializa el proceso de pago
- `ConfirmAsync()` - Confirma y procesa el pago

#### INiubizGateway
Interfaz para la comunicación con Niubiz:
- `GetSecurityTokenAsync()` - Obtiene token de seguridad
- `CreateSessionAsync()` - Crea sesión de pago
- `AuthorizeAsync()` - Autoriza la transacción

## 🔍 Endpoints de la API

### Productos
- `GET /` - Lista de productos (página principal)
- `GET /Products` - Lista de productos

### Checkout
- `GET /checkout/pay/{productId}` - Iniciar pago para un producto
- `POST /checkout/confirm` - Confirmar transacción de pago

## 🐛 Troubleshooting

### Problemas Comunes

#### Error: "Niubiz:MerchantId no configurado"
**Solución**: Verificar que las credenciales estén configuradas correctamente en user secrets.

#### Error de conexión con Niubiz
**Solución**: 
1. Verificar credenciales
2. Confirmar que la URL base sea correcta para el ambiente (QA/Producción)
3. Revisar los logs de la aplicación

#### Base de datos no se crea
**Solución**: La base de datos SQLite se crea automáticamente. Verificar permisos de escritura en el directorio.

### Logs

La aplicación registra información detallada sobre las transacciones. Revisar los logs en la consola para debugging.

## 🚦 Ambientes

### QA/Testing
- **Base URL**: `https://apitestenv.vnforapps.com`
- **JS Checkout**: `https://static-content-qas.vnforapps.com/v2/js/checkout.js`

### Producción
- **Base URL**: `https://api.vnforapps.com`
- **JS Checkout**: `https://static-content.vnforapps.com/v2/js/checkout.js`

## 📝 Licencia

Este proyecto es una demostración y está disponible para fines educativos y de prueba.

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📚 Documentación Completa

Este proyecto incluye documentación detallada para desarrolladores:

- 🏗️ **[Arquitectura del Sistema](docs/ARCHITECTURE.md)** - Detalles sobre Clean Architecture y estructura del código
- 💳 **[Integración Niubiz](docs/NIUBIZ_INTEGRATION.md)** - Guía completa de la integración con Niubiz
- 🚀 **[Guía de Desarrollo](docs/DEVELOPER_GUIDE.md)** - Setup, workflows y mejores prácticas
- 📋 **[Documentación de API](docs/API_DOCUMENTATION.md)** - Endpoints y ejemplos de uso
- 🔧 **[Troubleshooting](docs/TROUBLESHOOTING.md)** - Solución de problemas comunes y FAQ
- 📋 **[Changelog](CHANGELOG.md)** - Historial de cambios y versiones

## 📞 Soporte

### Recursos de Ayuda
- 📖 **Documentación**: Consulta las guías en la carpeta [`docs/`](docs/)
- 🐛 **Issues**: [Reportar problemas](https://github.com/eincioch/IntegracionNiubizDemo/issues)
- 💬 **Discusiones**: [GitHub Discussions](https://github.com/eincioch/IntegracionNiubizDemo/discussions)

### Enlaces Oficiales
- 🏦 **Niubiz**: [Documentación oficial](https://github.com/niubiz)
- 🔧 **.NET**: [Documentación de Microsoft](https://docs.microsoft.com/dotnet/)
- 💾 **Entity Framework**: [EF Core docs](https://docs.microsoft.com/ef/)

---

**Desarrollado con ❤️ para demostrar las mejores prácticas de integración con Niubiz**