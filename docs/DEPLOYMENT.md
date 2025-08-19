# Guía de Despliegue - IntegracionNiubizDemo

## Índice

1. [Requisitos del Sistema](#requisitos-del-sistema)
2. [Configuración de Ambientes](#configuración-de-ambientes)
3. [Despliegue Local](#despliegue-local)
4. [Despliegue en IIS](#despliegue-en-iis)
5. [Despliegue en Linux](#despliegue-en-linux)
6. [Despliegue en Docker](#despliegue-en-docker)
7. [Despliegue en Azure](#despliegue-en-azure)
8. [Configuración de Base de Datos](#configuración-de-base-de-datos)
9. [Variables de Entorno](#variables-de-entorno)
10. [Monitoreo y Logging](#monitoreo-y-logging)
11. [Seguridad](#seguridad)
12. [Troubleshooting](#troubleshooting)

---

## Requisitos del Sistema

### Requisitos Mínimos

| Componente | Especificación |
|------------|----------------|
| **Sistema Operativo** | Windows Server 2019+, Ubuntu 20.04+, CentOS 8+ |
| **Runtime** | .NET 9.0 Runtime |
| **Memoria RAM** | 2 GB mínimo, 4 GB recomendado |
| **Espacio en Disco** | 1 GB disponible |
| **Base de Datos** | SQLite (incluida) o SQL Server |
| **Conectividad** | HTTPS hacia Niubiz APIs |

### Requisitos de Red

```bash
# URLs que deben ser accesibles desde el servidor
https://apitestenv.vnforapps.com      # Ambiente QA
https://apiprod.vnforapps.com         # Ambiente Producción
https://static-content-qas.vnforapps.com  # JavaScript QA
https://static-content.vnforapps.com      # JavaScript Producción
```

### Puertos Requeridos

| Puerto | Protocolo | Descripción |
|--------|-----------|-------------|
| 80 | HTTP | Redirección a HTTPS |
| 443 | HTTPS | Tráfico web principal |
| 5000 | HTTP | Desarrollo local (opcional) |
| 5001 | HTTPS | Desarrollo local con SSL |

---

## Configuración de Ambientes

### Ambiente Desarrollo

```json
{
  "Environment": "Development",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "IntegracionNiubizDemo": "Debug"
    }
  },
  "Niubiz": {
    "Environment": "qa",
    "MerchantId": "400000181",
    "Username": "integraciones.visanet@necomplus.com",
    "Password": "d5e7nk$M",
    "Currency": "PEN"
  },
  "ConnectionStrings": {
    "Default": "Data Source=dev.db"
  }
}
```

### Ambiente Staging

```json
{
  "Environment": "Staging",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Niubiz": {
    "Environment": "qa",
    "MerchantId": "${NIUBIZ_MERCHANT_ID}",
    "Username": "${NIUBIZ_USERNAME}",
    "Password": "${NIUBIZ_PASSWORD}",
    "Currency": "PEN"
  },
  "ConnectionStrings": {
    "Default": "${CONNECTION_STRING}"
  }
}
```

### Ambiente Producción

```json
{
  "Environment": "Production",
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "Niubiz": {
    "Environment": "prod",
    "MerchantId": "${NIUBIZ_MERCHANT_ID}",
    "Username": "${NIUBIZ_USERNAME}",
    "Password": "${NIUBIZ_PASSWORD}",
    "Currency": "PEN"
  },
  "ConnectionStrings": {
    "Default": "${CONNECTION_STRING}"
  }
}
```

---

## Despliegue Local

### Opción 1: Visual Studio

1. **Abrir la solución**:
```bash
git clone https://github.com/eincioch/IntegracionNiubizDemo.git
cd IntegracionNiubizDemo
# Abrir IntegracionNiubizDemo.sln en Visual Studio
```

2. **Configurar User Secrets**:
```bash
# En Package Manager Console
dotnet user-secrets init --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:MerchantId" "SU_MERCHANT_ID" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Username" "SU_USUARIO" --project IntegracionNiubizDemo.Web
dotnet user-secrets set "Niubiz:Password" "SU_PASSWORD" --project IntegracionNiubizDemo.Web
```

3. **Ejecutar**:
   - Establecer `IntegracionNiubizDemo.Web` como proyecto de inicio
   - Presionar F5 o Ctrl+F5

### Opción 2: Línea de Comandos

```bash
# Clonar repositorio
git clone https://github.com/eincioch/IntegracionNiubizDemo.git
cd IntegracionNiubizDemo

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build --configuration Release

# Ejecutar
cd IntegracionNiubizDemo.Web
dotnet run
```

### Verificación Local

```bash
# Verificar que la aplicación está funcionando
curl -k https://localhost:5001/
curl -k https://localhost:5001/Products
```

---

## Despliegue en IIS

### Requisitos Previos

1. **Instalar .NET 9.0 Hosting Bundle**:
```powershell
# Descargar desde: https://dotnet.microsoft.com/download/dotnet/9.0
# Ejecutar: dotnet-hosting-9.0.x-win.exe
```

2. **Habilitar IIS**:
```powershell
# Ejecutar como Administrador
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45
```

### Publicar Aplicación

```bash
# Publicar aplicación
dotnet publish IntegracionNiubizDemo.Web/IntegracionNiubizDemo.Web.csproj -c Release -o C:\inetpub\wwwroot\IntegracionNiubizDemo
```

### Configurar Sitio Web en IIS

1. **Crear Application Pool**:
```powershell
# Abrir PowerShell como Administrador
Import-Module IISAdministration

# Crear Application Pool
New-IISAppPool -Name "IntegracionNiubizDemo" -ManagedRuntimeVersion ""
Set-IISAppPool -Name "IntegracionNiubizDemo" -ProcessModel.IdentityType ApplicationPoolIdentity
Set-IISAppPool -Name "IntegracionNiubizDemo" -Recycling.PeriodicRestart.Time "00:00:00"
```

2. **Crear Sitio Web**:
```powershell
# Crear sitio web
New-IISSite -Name "IntegracionNiubizDemo" -PhysicalPath "C:\inetpub\wwwroot\IntegracionNiubizDemo" -Port 80 -ApplicationPool "IntegracionNiubizDemo"

# Configurar HTTPS (opcional pero recomendado)
New-IISSiteBinding -Name "IntegracionNiubizDemo" -Protocol https -Port 443 -CertificateThumbprint "THUMBPRINT_DEL_CERTIFICADO"
```

### Configurar Variables de Entorno

```xml
<!-- web.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\IntegracionNiubizDemo.Web.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="NIUBIZ__MERCHANTID" value="SU_MERCHANT_ID" />
          <environmentVariable name="NIUBIZ__USERNAME" value="SU_USUARIO" />
          <environmentVariable name="NIUBIZ__PASSWORD" value="SU_PASSWORD" />
          <environmentVariable name="NIUBIZ__ENVIRONMENT" value="prod" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

---

## Despliegue en Linux

### Ubuntu 20.04 / 22.04

1. **Instalar .NET 9.0**:
```bash
# Agregar repositorio de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar .NET
sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get install -y dotnet-runtime-9.0 aspnetcore-runtime-9.0
```

2. **Crear usuario del servicio**:
```bash
sudo useradd -r -s /bin/false integracion-niubiz
sudo mkdir -p /opt/integracion-niubiz
sudo chown integracion-niubiz:integracion-niubiz /opt/integracion-niubiz
```

3. **Publicar y copiar aplicación**:
```bash
# En máquina de desarrollo
dotnet publish IntegracionNiubizDemo.Web/IntegracionNiubizDemo.Web.csproj -c Release -r linux-x64 --self-contained false -o ./publish

# Copiar al servidor
scp -r ./publish/* usuario@servidor:/opt/integracion-niubiz/
```

4. **Configurar servicio systemd**:
```bash
sudo nano /etc/systemd/system/integracion-niubiz.service
```

```ini
[Unit]
Description=Integración Niubiz Demo
Documentation=https://github.com/eincioch/IntegracionNiubizDemo
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/integracion-niubiz/IntegracionNiubizDemo.Web.dll
Restart=on-failure
RestartSec=5
TimeoutStopSec=90
KillSignal=SIGINT
SyslogIdentifier=integracion-niubiz
User=integracion-niubiz
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Variables de Niubiz
Environment=NIUBIZ__MERCHANTID=SU_MERCHANT_ID
Environment=NIUBIZ__USERNAME=SU_USUARIO
Environment=NIUBIZ__PASSWORD=SU_PASSWORD
Environment=NIUBIZ__ENVIRONMENT=prod

# Configuración de URLs
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

5. **Habilitar y iniciar servicio**:
```bash
sudo systemctl daemon-reload
sudo systemctl enable integracion-niubiz
sudo systemctl start integracion-niubiz
sudo systemctl status integracion-niubiz
```

### Configurar Nginx como Proxy Reverso

1. **Instalar Nginx**:
```bash
sudo apt update
sudo apt install nginx
```

2. **Configurar sitio**:
```bash
sudo nano /etc/nginx/sites-available/integracion-niubiz
```

```nginx
server {
    listen 80;
    server_name tu-dominio.com www.tu-dominio.com;
    
    # Redireccionar a HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name tu-dominio.com www.tu-dominio.com;

    # Configuración SSL
    ssl_certificate /etc/ssl/certs/tu-certificado.pem;
    ssl_certificate_key /etc/ssl/private/tu-clave-privada.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;

    # Headers de seguridad
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;
    add_header Content-Security-Policy "default-src 'self' http: https: data: blob: 'unsafe-inline'" always;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

3. **Habilitar sitio**:
```bash
sudo ln -s /etc/nginx/sites-available/integracion-niubiz /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Despliegue en Docker

### Dockerfile

```dockerfile
# IntegracionNiubizDemo/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["IntegracionNiubizDemo.Web/IntegracionNiubizDemo.Web.csproj", "IntegracionNiubizDemo.Web/"]
COPY ["IntegracionNiubizDemo.Application/IntegracionNiubizDemo.Application.csproj", "IntegracionNiubizDemo.Application/"]
COPY ["IntegracionNiubizDemo.Infrastructure/IntegracionNiubizDemo.Infrastructure.csproj", "IntegracionNiubizDemo.Infrastructure/"]
COPY ["IntegracionNiubizDemo.Persistence/IntegracionNiubizDemo.Persistence.csproj", "IntegracionNiubizDemo.Persistence/"]
COPY ["IntegracionNiubizDemo.Domain/IntegracionNiubizDemo.Domain.csproj", "IntegracionNiubizDemo.Domain/"]

RUN dotnet restore "IntegracionNiubizDemo.Web/IntegracionNiubizDemo.Web.csproj"
COPY . .
WORKDIR "/src/IntegracionNiubizDemo.Web"
RUN dotnet build "IntegracionNiubizDemo.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IntegracionNiubizDemo.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para la base de datos
RUN mkdir -p /app/data

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "IntegracionNiubizDemo.Web.dll"]
```

### Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  integracion-niubiz:
    build: .
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80
      - NIUBIZ__MERCHANTID=${NIUBIZ_MERCHANT_ID}
      - NIUBIZ__USERNAME=${NIUBIZ_USERNAME}
      - NIUBIZ__PASSWORD=${NIUBIZ_PASSWORD}
      - NIUBIZ__ENVIRONMENT=${NIUBIZ_ENVIRONMENT:-qa}
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - ./data:/app/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/"]
      interval: 30s
      timeout: 10s
      retries: 3

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/ssl:ro
    depends_on:
      - integracion-niubiz
    restart: unless-stopped
```

### Variables de Entorno (.env)

```bash
# .env
NIUBIZ_MERCHANT_ID=400000181
NIUBIZ_USERNAME=integraciones.visanet@necomplus.com
NIUBIZ_PASSWORD=d5e7nk$M
NIUBIZ_ENVIRONMENT=qa

# Para producción
# NIUBIZ_MERCHANT_ID=SU_MERCHANT_ID_PROD
# NIUBIZ_USERNAME=SU_USUARIO_PROD
# NIUBIZ_PASSWORD=SU_PASSWORD_PROD
# NIUBIZ_ENVIRONMENT=prod
```

### Comandos de Despliegue

```bash
# Construir imagen
docker build -t integracion-niubiz:latest .

# Ejecutar con Docker Compose
docker-compose up -d

# Ver logs
docker-compose logs -f integracion-niubiz

# Verificar estado
docker-compose ps

# Detener servicios
docker-compose down
```

---

## Despliegue en Azure

### Azure App Service

1. **Crear App Service**:
```bash
# Azure CLI
az login
az group create --name rg-integracion-niubiz --location "East US"

az appservice plan create \
  --name plan-integracion-niubiz \
  --resource-group rg-integracion-niubiz \
  --sku B1 \
  --is-linux

az webapp create \
  --resource-group rg-integracion-niubiz \
  --plan plan-integracion-niubiz \
  --name integracion-niubiz-app \
  --runtime "DOTNETCORE:9.0"
```

2. **Configurar variables de entorno**:
```bash
az webapp config appsettings set \
  --resource-group rg-integracion-niubiz \
  --name integracion-niubiz-app \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    NIUBIZ__MERCHANTID="SU_MERCHANT_ID" \
    NIUBIZ__USERNAME="SU_USUARIO" \
    NIUBIZ__PASSWORD="SU_PASSWORD" \
    NIUBIZ__ENVIRONMENT="prod"
```

3. **Desplegar desde GitHub**:
```bash
# Configurar despliegue continuo
az webapp deployment source config \
  --resource-group rg-integracion-niubiz \
  --name integracion-niubiz-app \
  --repo-url https://github.com/eincioch/IntegracionNiubizDemo \
  --branch main \
  --manual-integration
```

### Azure Container Instances

```bash
# Crear container instance
az container create \
  --resource-group rg-integracion-niubiz \
  --name integracion-niubiz-container \
  --image integracion-niubiz:latest \
  --dns-name-label integracion-niubiz \
  --ports 80 443 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    NIUBIZ__MERCHANTID=SU_MERCHANT_ID \
    NIUBIZ__USERNAME=SU_USUARIO \
    NIUBIZ__PASSWORD=SU_PASSWORD \
  --secure-environment-variables \
    NIUBIZ__PASSWORD=SU_PASSWORD_SEGURO
```

---

## Configuración de Base de Datos

### SQLite (Por Defecto)

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=/app/data/app.db"
  }
}
```

**Ventajas**:
- No requiere servidor de base de datos
- Configuración cero
- Ideal para desarrollo y aplicaciones pequeñas

**Limitaciones**:
- No soporta conexiones concurrentes de escritura
- Limitado para alta carga

### SQL Server

```json
{
  "ConnectionStrings": {
    "Default": "Server=servidor-sql;Database=IntegracionNiubiz;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Configuración**:
1. Crear base de datos
2. Ejecutar migraciones
3. Configurar usuario con permisos mínimos

```sql
-- Crear base de datos
CREATE DATABASE IntegracionNiubiz;

-- Crear usuario
CREATE LOGIN [niubiz_app] WITH PASSWORD = 'password_seguro';
USE IntegracionNiubiz;
CREATE USER [niubiz_app] FOR LOGIN [niubiz_app];

-- Asignar permisos mínimos
ALTER ROLE db_datareader ADD MEMBER [niubiz_app];
ALTER ROLE db_datawriter ADD MEMBER [niubiz_app];
```

### PostgreSQL

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=integracion_niubiz;Username=niubiz_user;Password=password_seguro"
  }
}
```

**Configuración**:
```sql
-- Crear base de datos y usuario
CREATE DATABASE integracion_niubiz;
CREATE USER niubiz_user WITH PASSWORD 'password_seguro';
GRANT ALL PRIVILEGES ON DATABASE integracion_niubiz TO niubiz_user;
```

---

## Variables de Entorno

### Variables Requeridas

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de ejecución | `Production` |
| `NIUBIZ__MERCHANTID` | ID del comercio | `400000181` |
| `NIUBIZ__USERNAME` | Usuario de Niubiz | `usuario@merchant.com` |
| `NIUBIZ__PASSWORD` | Contraseña de Niubiz | `password123` |
| `NIUBIZ__ENVIRONMENT` | Ambiente Niubiz | `prod` o `qa` |

### Variables Opcionales

| Variable | Descripción | Por Defecto |
|----------|-------------|-------------|
| `NIUBIZ__CURRENCY` | Moneda | `PEN` |
| `ConnectionStrings__Default` | Cadena conexión DB | `Data Source=app.db` |
| `ASPNETCORE_URLS` | URLs de bind | `http://+:80` |

### Configuración por Ambiente

#### Desarrollo
```bash
export ASPNETCORE_ENVIRONMENT=Development
export NIUBIZ__ENVIRONMENT=qa
export NIUBIZ__MERCHANTID=400000181
```

#### Staging
```bash
export ASPNETCORE_ENVIRONMENT=Staging
export NIUBIZ__ENVIRONMENT=qa
export NIUBIZ__MERCHANTID=SU_MERCHANT_ID
```

#### Producción
```bash
export ASPNETCORE_ENVIRONMENT=Production
export NIUBIZ__ENVIRONMENT=prod
export NIUBIZ__MERCHANTID=SU_MERCHANT_ID_PROD
```

---

## Monitoreo y Logging

### Configuración de Serilog

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### Health Checks

```csharp
// En Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddUrlGroup(new Uri("https://apiprod.vnforapps.com/api.security/v1/security"), "Niubiz API");

app.MapHealthChecks("/health");
```

### Métricas de Application Insights

```csharp
// En Program.cs
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);
```

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://..."
  }
}
```

---

## Seguridad

### HTTPS Configuration

```csharp
// En Program.cs para producción
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

### Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});
```

### Rate Limiting

```csharp
// Instalar: AspNetCoreRateLimit
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 60
        }
    };
});
```

---

## Troubleshooting

### Problemas Comunes

#### 1. Error "Unable to start Kestrel"

**Síntoma**: La aplicación no inicia
```
Unable to start Kestrel.
System.IO.IOException: Failed to bind to address
```

**Solución**:
```bash
# Verificar que el puerto no esté en uso
netstat -tulpn | grep :80
netstat -tulpn | grep :443

# Cambiar puerto en appsettings.json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

#### 2. Error de conexión a Niubiz

**Síntoma**: Timeouts o errores 403
```
HttpRequestException: The SSL connection could not be established
```

**Solución**:
```bash
# Verificar conectividad
curl -v https://apitestenv.vnforapps.com/api.security/v1/security

# Verificar certificados SSL
openssl s_client -connect apitestenv.vnforapps.com:443

# Configurar proxy si es necesario
export https_proxy=http://proxy:8080
```

#### 3. Base de datos bloqueada (SQLite)

**Síntoma**: Database is locked
```
Microsoft.Data.Sqlite.SqliteException: SQLite Error 5: 'database is locked'
```

**Solución**:
```bash
# Verificar procesos que usan la DB
lsof /path/to/app.db

# Reiniciar aplicación
sudo systemctl restart integracion-niubiz

# Usar SQL Server para alta concurrencia
```

### Logs de Diagnóstico

```bash
# Ver logs del servicio (Linux)
journalctl -u integracion-niubiz -f

# Ver logs de aplicación
tail -f /app/logs/log-$(date +%Y%m%d).txt

# Verificar estado del servicio
systemctl status integracion-niubiz

# Verificar conectividad de red
curl -I https://apiprod.vnforapps.com/
```

### Scripts de Monitoreo

```bash
#!/bin/bash
# monitor.sh - Script de monitoreo básico

APP_URL="https://tu-dominio.com"
LOG_FILE="/var/log/monitor.log"

# Verificar que la aplicación responde
if curl -s --head "$APP_URL" | head -n 1 | grep -q "200 OK"; then
    echo "$(date): App OK" >> $LOG_FILE
else
    echo "$(date): App DOWN - Restarting service" >> $LOG_FILE
    systemctl restart integracion-niubiz
fi

# Verificar uso de disco
DISK_USAGE=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
if [ $DISK_USAGE -gt 80 ]; then
    echo "$(date): Warning - Disk usage is ${DISK_USAGE}%" >> $LOG_FILE
fi
```

---

## Scripts de Automatización

### Script de Despliegue

```bash
#!/bin/bash
# deploy.sh

set -e

APP_NAME="integracion-niubiz"
APP_PATH="/opt/$APP_NAME"
BACKUP_PATH="/opt/backups"
SERVICE_NAME="$APP_NAME"

echo "Starting deployment..."

# Crear backup
sudo mkdir -p $BACKUP_PATH
sudo cp -r $APP_PATH $BACKUP_PATH/$APP_NAME-$(date +%Y%m%d-%H%M%S)

# Detener servicio
sudo systemctl stop $SERVICE_NAME

# Actualizar aplicación
sudo rm -rf $APP_PATH/*
sudo cp -r ./publish/* $APP_PATH/
sudo chown -R $APP_NAME:$APP_NAME $APP_PATH

# Iniciar servicio
sudo systemctl start $SERVICE_NAME
sudo systemctl status $SERVICE_NAME

echo "Deployment completed successfully!"
```

### Script de Backup

```bash
#!/bin/bash
# backup.sh

BACKUP_DIR="/opt/backups"
DATE=$(date +%Y%m%d_%H%M%S)
DB_PATH="/opt/integracion-niubiz/app.db"

# Crear directorio de backup
mkdir -p $BACKUP_DIR

# Backup de base de datos
if [ -f "$DB_PATH" ]; then
    cp "$DB_PATH" "$BACKUP_DIR/app_$DATE.db"
    echo "Database backup created: app_$DATE.db"
fi

# Limpiar backups antiguos (mantener 30 días)
find $BACKUP_DIR -name "app_*.db" -mtime +30 -delete

echo "Backup completed successfully!"
```

---

## Conclusión

Esta guía proporciona múltiples opciones de despliegue para **IntegracionNiubizDemo**, desde desarrollo local hasta producción en la nube. Cada ambiente tiene sus consideraciones específicas de seguridad, performance y mantenibilidad.

### Recomendaciones por Escenario:

- **Desarrollo**: Visual Studio + User Secrets
- **Testing**: Docker Compose
- **Staging**: Linux + Nginx + Systemd
- **Producción**: Azure App Service o IIS con SSL

Siempre configure monitoreo, logging y backups apropiados para su ambiente de producción.