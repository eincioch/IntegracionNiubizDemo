using Microsoft.Extensions.DependencyInjection;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Application.Services;

namespace IntegracionNiubizDemo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICheckoutService, CheckoutService>(); // <-- registro faltante
        return services;
    }
}
