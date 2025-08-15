using IntegracionNiubizDemo.Application;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Infrastructure;
using IntegracionNiubizDemo.Persistence.Data;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

// Configuración/Secrets si aplica...
builder.Services.AddControllersWithViews();

// REGISTRA autorización (y autenticación si luego la usas)
builder.Services.AddAuthorization();
// builder.Services.AddAuthentication(/* esquema por defecto */);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ...otros middlewares
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// AUTENTICACIÓN debe ir antes de autorización
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

// Seed, migraciones, etc.
app.Run();
