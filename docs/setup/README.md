# 🚀 Guía de Configuración e Instalación

## Requisitos del Sistema

### Software Requerido

| Componente | Versión Mínima | Recomendada | Notas |
|------------|----------------|-------------|-------|
| **.NET SDK** | 8.0 | 8.0.x (latest) | Para compilación y ejecución |
| **IDE** | - | Visual Studio 2022 / VS Code | Para desarrollo |
| **Git** | 2.30+ | Latest | Para control de versiones |
| **SQLite** | 3.35+ | Latest | Base de datos (incluida en .NET) |

### Cuentas y Servicios

- **Cuenta Niubiz**: Registro en [Niubiz Developers](https://www.niubiz.com.pe/desarrolladores)
- **Credenciales de Sandbox**: Para pruebas en ambiente QA
- **Credenciales de Producción**: Para despliegue en vivo

## 📥 Instalación del Proyecto

### 1. Clonar el Repositorio

```bash
# Clonar el repositorio
git clone https://github.com/eincioch/IntegracionNiubizDemo.git

# Navegar al directorio
cd IntegracionNiubizDemo

# Verificar la estructura
ls -la
```

### 2. Verificar Instalación de .NET

```bash
# Verificar versión de .NET
dotnet --version

# Debería mostrar 8.0.x o superior
# Si no tienes .NET 8, descárgalo desde: https://dotnet.microsoft.com/download
```

### 3. Restaurar Dependencias

```bash
# Restaurar paquetes NuGet
dotnet restore

# Verificar que no hay errores
echo $?  # Debería mostrar 0
```

### 4. Compilar el Proyecto

```bash
# Compilar en modo Debug
dotnet build

# Compilar en modo Release (para producción)
dotnet build --configuration Release
```

## ⚙️ Configuración de Niubiz

### Obtener Credenciales de Desarrollo

1. **Registrarse en Niubiz**:
   - Visita [Niubiz Developers](https://www.niubiz.com.pe/desarrolladores)
   - Crea una cuenta de desarrollador
   - Solicita acceso al ambiente de sandbox

2. **Obtener Credenciales QA**:
   - **Merchant ID**: Identificador único de tu comercio
   - **Username**: Usuario API proporcionado por Niubiz
   - **Password**: Contraseña API proporcionada por Niubiz

### Configuración por Ambientes

#### Ambiente de Desarrollo (Sandbox/QA)

**Opción 1: User Secrets (Recomendado para desarrollo)**

```bash
# Navegar al proyecto Web
cd IntegracionNiubizDemo.Web

# Configurar las credenciales
dotnet user-secrets set "Niubiz:Environment" "qa"
dotnet user-secrets set "Niubiz:MerchantId" "TU_MERCHANT_ID_QA"
dotnet user-secrets set "Niubiz:Username" "TU_USERNAME_QA"
dotnet user-secrets set "Niubiz:Password" "TU_PASSWORD_QA"
dotnet user-secrets set "Niubiz:Currency" "PEN"

# Verificar configuración
dotnet user-secrets list
```

**Opción 2: appsettings.Development.json**

Crear archivo `IntegracionNiubizDemo.Web/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "IntegracionNiubizDemo.Infrastructure.Niubiz": "Debug"
    }
  },
  "Niubiz": {
    "Environment": "qa",
    "MerchantId": "TU_MERCHANT_ID_QA",
    "Username": "TU_USERNAME_QA",
    "Password": "TU_PASSWORD_QA",
    "Currency": "PEN",
    "BaseUrls": {
      "qa": "https://apisandbox.vnforapps.com",
      "prod": "https://apiprod.vnforapps.com"
    },
    "StaticContent": {
      "qa": "https://static-content-qas.vnforapps.com/v2/js/checkout.js?qa=true",
      "prod": "https://static-content.vnforapps.com/v2/js/checkout.js"
    }
  }
}
```

#### Ambiente de Producción

**Variables de Entorno (Recomendado para producción)**

```bash
# Linux/macOS
export NIUBIZ__ENVIRONMENT=prod
export NIUBIZ__MERCHANTID=TU_MERCHANT_ID_PROD
export NIUBIZ__USERNAME=TU_USERNAME_PROD
export NIUBIZ__PASSWORD=TU_PASSWORD_PROD

# Windows PowerShell
$env:NIUBIZ__ENVIRONMENT="prod"
$env:NIUBIZ__MERCHANTID="TU_MERCHANT_ID_PROD"
$env:NIUBIZ__USERNAME="TU_USERNAME_PROD"
$env:NIUBIZ__PASSWORD="TU_PASSWORD_PROD"
```

**appsettings.Production.json**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "IntegracionNiubizDemo": "Information"
    }
  },
  "Niubiz": {
    "Environment": "prod",
    "Currency": "PEN",
    "BaseUrls": {
      "qa": "https://apisandbox.vnforapps.com",
      "prod": "https://apiprod.vnforapps.com"
    },
    "StaticContent": {
      "qa": "https://static-content-qas.vnforapps.com/v2/js/checkout.js?qa=true",
      "prod": "https://static-content.vnforapps.com/v2/js/checkout.js"
    }
  }
}
```

## 🗄️ Configuración de Base de Datos

### SQLite (Configuración por Defecto)

El proyecto usa SQLite por simplicidad. La configuración por defecto es:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=app.db"
  }
}
```

### SQL Server (Opcional)

Para usar SQL Server, actualiza la configuración:

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=IntegracionNiubizDemo;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Instalar paquete:**
```bash
dotnet add IntegracionNiubizDemo.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
```

**Actualizar DependencyInjection.cs:**
```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
```

### MySQL (Opcional)

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=IntegracionNiubizDemo;Uid=root;Pwd=password;"
  }
}
```

**Instalar paquete:**
```bash
dotnet add IntegracionNiubizDemo.Infrastructure package Pomelo.EntityFrameworkCore.MySql
```

## 🚀 Ejecutar la Aplicación

### Modo Desarrollo

```bash
# Desde la raíz del proyecto
dotnet run --project IntegracionNiubizDemo.Web

# O navegar al proyecto Web
cd IntegracionNiubizDemo.Web
dotnet run
```

**Salida esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Modo Producción

```bash
# Compilar para producción
dotnet build --configuration Release

# Ejecutar en modo producción
dotnet run --project IntegracionNiubizDemo.Web --configuration Release
```

### Con Watch (Auto-reload)

```bash
# Para desarrollo con recarga automática
dotnet watch run --project IntegracionNiubizDemo.Web
```

## 🧪 Verificar la Instalación

### 1. Verificar que la Aplicación Inicia

Abrir navegador en: `https://localhost:7001`

Deberías ver la página principal con lista de productos.

### 2. Verificar Configuración de Niubiz

Crear un archivo temporal para verificar la configuración:

```bash
# Crear archivo de verificación
cat > verify-config.cs << 'EOF'
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IntegracionNiubizDemo.Infrastructure.Niubiz;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<NiubizOptions>(builder.Configuration.GetSection("Niubiz"));

var host = builder.Build();
var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<NiubizOptions>>().Value;

Console.WriteLine($"Environment: {options.Environment}");
Console.WriteLine($"MerchantId: {options.MerchantId}");
Console.WriteLine($"Username: {options.Username}");
Console.WriteLine($"BaseUrl: {options.BaseUrl}");

Console.WriteLine(string.IsNullOrEmpty(options.MerchantId) ? "❌ Configuración incompleta" : "✅ Configuración OK");
EOF

# Ejecutar verificación
dotnet script verify-config.cs

# Limpiar
rm verify-config.cs
```

### 3. Probar Endpoints Básicos

```bash
# Verificar endpoint principal
curl -k https://localhost:7001

# Debería retornar HTML con lista de productos
```

### 4. Verificar Logs

Los logs se muestran en la consola por defecto. Para logs más detallados:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

## 🐳 Configuración con Docker

### Dockerfile

Crear `Dockerfile` en la raíz del proyecto:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY *.sln .
COPY IntegracionNiubizDemo.Domain/*.csproj IntegracionNiubizDemo.Domain/
COPY IntegracionNiubizDemo.Application/*.csproj IntegracionNiubizDemo.Application/
COPY IntegracionNiubizDemo.Infrastructure/*.csproj IntegracionNiubizDemo.Infrastructure/
COPY IntegracionNiubizDemo.Persistence/*.csproj IntegracionNiubizDemo.Persistence/
COPY IntegracionNiubizDemo.Web/*.csproj IntegracionNiubizDemo.Web/

# Restaurar dependencias
RUN dotnet restore

# Copiar código fuente
COPY . .

# Compilar aplicación
RUN dotnet build --configuration Release --no-restore
RUN dotnet publish IntegracionNiubizDemo.Web --configuration Release --no-build --output /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Configurar puertos
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "IntegracionNiubizDemo.Web.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - NIUBIZ__ENVIRONMENT=qa
      - NIUBIZ__MERCHANTID=${MERCHANT_ID}
      - NIUBIZ__USERNAME=${USERNAME}
      - NIUBIZ__PASSWORD=${PASSWORD}
    volumes:
      - ./data:/app/data
```

### .env file

```bash
MERCHANT_ID=tu_merchant_id_aqui
USERNAME=tu_username_aqui
PASSWORD=tu_password_aqui
```

### Comandos Docker

```bash
# Construir imagen
docker build -t niubiz-demo .

# Ejecutar contenedor
docker run -p 8080:80 \
  -e NIUBIZ__MERCHANTID=tu_merchant_id \
  -e NIUBIZ__USERNAME=tu_username \
  -e NIUBIZ__PASSWORD=tu_password \
  niubiz-demo

# Con docker-compose
docker-compose up -d
```

## 🔒 Configuraciones de Seguridad

### HTTPS en Desarrollo

Generar certificados de desarrollo:

```bash
# Generar certificado HTTPS para desarrollo
dotnet dev-certs https --trust
```

### Configuración de Headers de Seguridad

En `Program.cs`:

```csharp
// Configurar headers de seguridad
app.UseHsts();
app.UseHttpsRedirection();

// Headers de seguridad adicionales
app.Use((context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    return next();
});
```

### Configuración de CORS

```csharp
services.AddCors(options =>
{
    options.AddPolicy("NiubizPolicy", policy =>
    {
        policy.WithOrigins("https://static-content-qas.vnforapps.com", 
                          "https://static-content.vnforapps.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("NiubizPolicy");
```

## 🔧 Configuraciones Avanzadas

### Health Checks

```csharp
services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddUrlGroup(new Uri($"{niubizBaseUrl}/api.security/v1/security"), 
                 "Niubiz API", 
                 HttpMethod.Post,
                 tags: new[] { "niubiz" });

app.MapHealthChecks("/health");
```

### Logging Avanzado

**appsettings.json:**
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/niubiz-demo-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### Rate Limiting

```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("CheckoutPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// En el controller
[EnableRateLimiting("CheckoutPolicy")]
public class CheckoutController : Controller { }
```

## ❓ Troubleshooting de Configuración

### Error: "Unable to configure HTTPS endpoint"

**Solución:**
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Error: "Niubiz credentials not configured"

**Verificar:**
1. Variables de entorno establecidas
2. User secrets configurados
3. appsettings.json correcto

**Debug:**
```bash
dotnet user-secrets list --project IntegracionNiubizDemo.Web
```

### Error: "Database connection failed"

**Verificar:**
1. String de conexión correcto
2. Permisos de escritura en directorio
3. SQLite instalado (normalmente incluido en .NET)

### Error: "Port already in use"

**Cambiar puerto:**
```bash
dotnet run --project IntegracionNiubizDemo.Web --urls "https://localhost:7002;http://localhost:5002"
```

### Performance en Desarrollo

**Optimizar para desarrollo:**
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

## 📋 Checklist de Configuración

- [ ] ✅ .NET 8 SDK instalado
- [ ] ✅ Repositorio clonado
- [ ] ✅ Dependencias restauradas (`dotnet restore`)
- [ ] ✅ Proyecto compilado (`dotnet build`)
- [ ] ✅ Credenciales Niubiz configuradas
- [ ] ✅ Base de datos configurada
- [ ] ✅ Aplicación ejecutándose
- [ ] ✅ Página principal accesible
- [ ] ✅ Logs funcionando correctamente
- [ ] ✅ HTTPS configurado
- [ ] ✅ Health checks respondiendo (opcional)

Una vez completado este checklist, tu instalación estará lista para desarrollo y testing.