# 🔧 Troubleshooting & FAQ

## Problemas Comunes y Soluciones

### 🚨 Errores de Configuración

#### Error: "Niubiz:MerchantId no configurado"

**Síntomas**:
```
System.ComponentModel.DataAnnotations.ValidationException: Niubiz:MerchantId no configurado
```

**Causas posibles**:
- User secrets no configurados
- Configuración en appsettings incompleta
- Variables de entorno no definidas

**Soluciones**:

1. **Verificar User Secrets**:
```bash
# Listar secrets configurados
dotnet user-secrets list --project IntegracionNiubizDemo.Web

# Si está vacío, configurar:
dotnet user-secrets set "Niubiz:MerchantId" "TU_MERCHANT_ID" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Username" "TU_USERNAME" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Password" "TU_PASSWORD" --project IntegracionNiubizDemo.Web
```

2. **Verificar appsettings.Development.json**:
```json
{
  "Niubiz": {
    "Environment": "qa",
    "BaseUrl": "https://apitestenv.vnforapps.com",
    // MerchantId, Username, Password deben estar en user secrets
  }
}
```

3. **En producción, usar variables de entorno**:
```bash
export Niubiz__MerchantId="TU_MERCHANT_ID"
export Niubiz__Username="TU_USERNAME"
export Niubiz__Password="TU_PASSWORD"
```

#### Error: "No se puede conectar a la base de datos"

**Síntomas**:
```
Microsoft.Data.Sqlite.SqliteException: SQLite Error 14: 'unable to open database file'
```

**Soluciones**:

1. **Verificar permisos del directorio**:
```bash
# Asegurar que el directorio tenga permisos de escritura
chmod 755 ./
```

2. **Verificar cadena de conexión**:
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=app.db"
  }
}
```

3. **Recrear la base de datos**:
```bash
# Eliminar base de datos existente
rm app.db

# Reiniciar la aplicación para que se recree automáticamente
dotnet run --project IntegracionNiubizDemo.Web
```

### 🌐 Errores de Integración Niubiz

#### Error: "401 Unauthorized" al obtener Security Token

**Síntomas**:
```
InvalidOperationException: Error obteniendo security token: Unauthorized - {"errorCode":"401","errorMessage":"Invalid credentials"}
```

**Causas**:
- Credenciales incorrectas
- Usuario no autorizado para el ambiente
- Password expirado

**Soluciones**:

1. **Verificar credenciales**:
```bash
# Listar y verificar user secrets
dotnet user-secrets list --project IntegracionNiubizDemo.Web

# Probar credenciales manualmente
curl -X POST "https://apitestenv.vnforapps.com/api.security/v1/security" \
  -H "Authorization: Basic $(echo -n 'username:password' | base64)" \
  -H "Content-Type: application/json" \
  -d '{}'
```

2. **Verificar ambiente**:
```json
{
  "Niubiz": {
    "Environment": "qa",  // Debe coincidir con las credenciales
    "BaseUrl": "https://apitestenv.vnforapps.com"  // QA endpoint
  }
}
```

3. **Contactar soporte de Niubiz** si las credenciales son correctas.

#### Error: "Session Key inválida" en el formulario de pago

**Síntomas**:
- El formulario de Niubiz no se carga
- Error en la consola del navegador: "Invalid session key"

**Soluciones**:

1. **Verificar configuración del JavaScript**:
```html
<!-- Verificar que los valores no estén vacíos -->
<script>
window.VisanetCheckout = {
    settings: {
        merchantId: '@Model.MerchantId',      // No debe estar vacío
        sessionKey: '@Model.SessionKey',      // No debe estar vacío
        amount: '@Model.Amount.ToString("F2")', // Formato correcto
        currency: '@Model.Currency'           // PEN o USD
    }
};
</script>
```

2. **Verificar URL del JavaScript**:
```json
{
  "Niubiz": {
    "StaticJsUrl": "https://static-content-qas.vnforapps.com/v2/js/checkout.js"
  }
}
```

3. **Revisar logs de la aplicación** para ver errores en la creación de sesión.

#### Error: "Transaction Token no válido" en confirmación

**Síntomas**:
```
ConfirmResult { Success = false, Message = "Token de transacción inválido" }
```

**Causas**:
- Token expirado (timeout de 15 minutos)
- Token ya utilizado
- Token corrupto en transmisión

**Soluciones**:

1. **Verificar que el token llegue correctamente**:
```javascript
// En el callback de Niubiz
onComplete: function(token) {
    console.log('Token recibido:', token); // Debug
    document.getElementById('transactionToken').value = token;
    document.getElementById('pay-button').click();
}
```

2. **Verificar timing**:
- El usuario debe completar el pago dentro de 15 minutos
- No recargar la página después de obtener el token

3. **Implementar manejo de timeout**:
```javascript
// Mostrar mensaje si pasa mucho tiempo
setTimeout(function() {
    alert('Sesión expirada. Por favor, inicia el proceso nuevamente.');
}, 900000); // 15 minutos
```

### 🔧 Errores de Desarrollo

#### Error: "NETSDK1045: .NET SDK no soporta .NET 9.0"

**Síntomas**:
```
error NETSDK1045: The current .NET SDK does not support targeting .NET 9.0
```

**Solución**:
```bash
# Verificar versión del SDK
dotnet --version

# Si es menor a 9.0, cambiar target framework en todos los .csproj
# De: <TargetFramework>net9.0</TargetFramework>
# A:  <TargetFramework>net8.0</TargetFramework>
```

#### Error: "The type or namespace 'Product' could not be found"

**Síntomas**:
- Errores de compilación sobre tipos no encontrados
- Referencias de proyecto rotas

**Soluciones**:

1. **Verificar referencias de proyecto**:
```bash
# Listar referencias
dotnet list IntegracionNiubizDemo.Web reference

# Agregar referencia faltante si es necesario
dotnet add IntegracionNiubizDemo.Web reference IntegracionNiubizDemo.Application
```

2. **Limpiar y reconstruir**:
```bash
dotnet clean
dotnet restore
dotnet build
```

#### Error: "Could not load file or assembly"

**Síntomas**:
```
FileNotFoundException: Could not load file or assembly 'Microsoft.EntityFrameworkCore'
```

**Soluciones**:

1. **Restaurar paquetes NuGet**:
```bash
dotnet restore
```

2. **Verificar versiones de paquetes**:
```xml
<!-- Asegurar que todas las versiones de EF Core coincidan -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.7" />
```

3. **Limpiar caché de NuGet**:
```bash
dotnet nuget locals all --clear
dotnet restore
```

### 📱 Errores de UI/Frontend

#### El formulario de pago no aparece

**Diagnóstico**:

1. **Verificar consola del navegador** (F12):
```javascript
// Errores comunes:
// - CORS error loading Niubiz JS
// - SSL certificate error
// - Network timeout
```

2. **Verificar Network tab** en DevTools:
- Request a Niubiz JS debe ser 200 OK
- No debe haber errores CORS

**Soluciones**:

1. **Para desarrollo local con HTTPS**:
```bash
# Confiar en el certificado de desarrollo
dotnet dev-certs https --trust
```

2. **Verificar configuración CORS** (si es necesario):
```csharp
// Program.cs
app.UseCors(policy => policy
    .WithOrigins("https://static-content-qas.vnforapps.com")
    .AllowAnyMethod()
    .AllowAnyHeader());
```

#### Botón "Comprar" no funciona

**Diagnóstico**:
```html
<!-- Verificar que el enlace esté bien formado -->
<a href="/checkout/pay/@product.Id" class="btn btn-primary">Comprar</a>
```

**Soluciones**:

1. **Verificar routing**:
```csharp
// Program.cs - verificar que el routing esté configurado
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");
```

2. **Verificar que el ID del producto sea válido**:
```csharp
// En la vista, verificar que product.Id no sea Guid.Empty
@if (product.Id != Guid.Empty)
{
    <a href="/checkout/pay/@product.Id" class="btn btn-primary">Comprar</a>
}
```

## 🧪 Herramientas de Diagnóstico

### Verificar Configuración

```bash
# Script para verificar configuración completa
#!/bin/bash

echo "=== Verificación de Configuración ==="

# 1. Verificar .NET SDK
echo "1. Versión de .NET SDK:"
dotnet --version

# 2. Verificar user secrets
echo "2. User Secrets configurados:"
dotnet user-secrets list --project IntegracionNiubizDemo.Web

# 3. Verificar que compila
echo "3. Verificando compilación:"
dotnet build --no-restore

# 4. Verificar conectividad con Niubiz (requiere credenciales)
echo "4. Testing conectividad con Niubiz..."
curl -s -o /dev/null -w "%{http_code}" https://apitestenv.vnforapps.com/api.security/v1/security
```

### Logs de Debugging

**Activar logs detallados**:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "IntegracionNiubizDemo": "Trace",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Revisar logs específicos**:

```bash
# Filtrar logs por categoría
dotnet run --project IntegracionNiubizDemo.Web 2>&1 | grep "IntegracionNiubizDemo"

# Logs de Entity Framework
dotnet run --project IntegracionNiubizDemo.Web 2>&1 | grep "Microsoft.EntityFrameworkCore"
```

### Testing de Endpoints

**Script de prueba básica**:

```bash
#!/bin/bash
BASE_URL="https://localhost:5001"

echo "=== Testing Endpoints ==="

# 1. Página principal
echo "1. Testing página principal..."
curl -k -s -o /dev/null -w "Status: %{http_code}\n" "$BASE_URL/"

# 2. Lista de productos
echo "2. Testing lista de productos..."
curl -k -s -o /dev/null -w "Status: %{http_code}\n" "$BASE_URL/Products"

# 3. Checkout (requiere product ID válido)
# echo "3. Testing checkout..."
# curl -k -s -o /dev/null -w "Status: %{http_code}\n" "$BASE_URL/checkout/pay/GUID-AQUI"
```

## ❓ FAQ (Preguntas Frecuentes)

### Q: ¿Puedo usar este código en producción?

**A**: Este es un proyecto de demostración. Para producción necesitarías:
- ✅ Credenciales de producción de Niubiz
- ✅ Base de datos robusta (SQL Server, PostgreSQL)
- ✅ Implementar logging estructurado
- ✅ Agregar health checks
- ✅ Implementar rate limiting
- ✅ Configurar HTTPS correctamente
- ✅ Agregar manejo de errores más robusto

### Q: ¿Cómo obtengo credenciales de Niubiz?

**A**: Debes:
1. Contactar a Niubiz/VisaNet Perú
2. Completar el proceso de afiliación como comercio
3. Recibir credenciales para ambiente de pruebas (QA)
4. Después de testing, recibir credenciales de producción

### Q: ¿El proyecto funciona con otras pasarelas de pago?

**A**: La arquitectura Clean facilita agregar otras pasarelas:
1. Crear nueva implementación de `IPaymentGateway`
2. Implementar lógica específica del proveedor
3. Registrar en DI container
4. Potencialmente agregar factory pattern para múltiples gateways

### Q: ¿Cómo agrego autenticación de usuarios?

**A**: Puedes integrar ASP.NET Core Identity:
```csharp
// Program.cs
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

app.UseAuthentication();
app.UseAuthorization();
```

### Q: ¿Cómo manejo múltiples monedas?

**A**: 
1. Modificar `NiubizOptions` para soportar múltiples monedas
2. Actualizar `Product` para incluir moneda
3. Modificar lógica de checkout para usar moneda del producto
4. Verificar que Niubiz soporte la moneda en tu afiliación

### Q: ¿Cómo implemento webhooks de Niubiz?

**A**: Niubiz puede enviar notificaciones de estado:
```csharp
[HttpPost("/webhooks/niubiz")]
public async Task<IActionResult> NiubizWebhook([FromBody] NiubizWebhookModel model)
{
    // Verificar firma del webhook
    // Actualizar estado de la transacción
    // Notificar al usuario si es necesario
    return Ok();
}
```

### Q: ¿Cómo implemento reembolsos?

**A**: Niubiz tiene endpoints para reembolsos:
1. Agregar método `RefundAsync` a `INiubizGateway`
2. Implementar endpoint `/api.authorization/v3/void/{merchantId}`
3. Agregar UI para gestión de reembolsos

### Q: ¿Cómo escalo la aplicación?

**A**: Consideraciones para escalabilidad:
- ✅ Usar base de datos externa (no SQLite)
- ✅ Implementar caché distribuido (Redis)
- ✅ Separar en microservicios si es necesario
- ✅ Usar load balancer
- ✅ Implementar circuit breaker para Niubiz
- ✅ Monitoring y alertas

## 📞 Obtener Ayuda

### Recursos Oficiales
- **Niubiz**: [Documentación oficial](https://github.com/niubiz)
- **.NET**: [Documentación de Microsoft](https://docs.microsoft.com/dotnet/)
- **EF Core**: [Entity Framework docs](https://docs.microsoft.com/ef/)

### Community
- **Stack Overflow**: Usar tags `niubiz`, `aspnet-core`, `entity-framework-core`
- **GitHub Issues**: [Reportar problemas específicos del proyecto](https://github.com/eincioch/IntegracionNiubizDemo/issues)

### Logging para Soporte

Si necesitas ayuda, incluye:

1. **Versión de .NET**: `dotnet --version`
2. **Sistema operativo**: Windows/Linux/macOS + versión
3. **Logs relevantes** (sin credenciales)
4. **Pasos para reproducir** el problema
5. **Configuración utilizada** (sin credenciales sensibles)

**Formato de log para soporte**:
```
[2023-12-01 10:30:15] [ERROR] IntegracionNiubizDemo.Infrastructure.Niubiz.NiubizClient
Error obteniendo security token: Unauthorized
Request: POST https://apitestenv.vnforapps.com/api.security/v1/security
Headers: Authorization=Basic [REDACTED], Accept=application/json
Response: 401 {"errorCode":"401","errorMessage":"Invalid credentials"}
```