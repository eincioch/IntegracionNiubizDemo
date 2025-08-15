using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IntegracionNiubizDemo.Persistence.Data;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Persistence.Repositories;
using IntegracionNiubizDemo.Infrastructure.Niubiz;

namespace IntegracionNiubizDemo.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra persistencia (DbContext + repositorios) y la integración Niubiz (opciones + HttpClient).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddPersistence(configuration);
        services.AddNiubiz(configuration);

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = "Data Source=app.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }

    private static IServiceCollection AddNiubiz(this IServiceCollection services, IConfiguration configuration)
    {
        var env = configuration["Niubiz:Environment"] ?? "qa";
        var baseUrl = configuration[$"Niubiz:BaseUrls:{env}"]
            ?? throw new InvalidOperationException("BaseUrl Niubiz no configurado");
        var staticJs = configuration[$"Niubiz:StaticContent:{env}"]
            ?? throw new InvalidOperationException("Static JS Niubiz no configurado");

        services.AddOptions<NiubizOptions>()
            .Configure(opt =>
            {
                opt.Environment = env;
                opt.MerchantId = configuration["Niubiz:MerchantId"] ?? "";
                opt.Username   = configuration["Niubiz:Username"]   ?? "";
                opt.Password   = configuration["Niubiz:Password"]   ?? "";
                opt.Currency   = configuration["Niubiz:Currency"]   ?? "PEN";
                opt.BaseUrl    = baseUrl;
                opt.StaticJsUrl= staticJs;
                opt.SecurityEndpoint      = configuration["Niubiz:Endpoints:Security"]      ?? opt.SecurityEndpoint;
                opt.SessionEndpoint       = configuration["Niubiz:Endpoints:Session"]       ?? opt.SessionEndpoint;
                opt.AuthorizationEndpoint = configuration["Niubiz:Endpoints:Authorization"] ?? opt.AuthorizationEndpoint;
            })
            .Validate(o => !string.IsNullOrWhiteSpace(o.MerchantId) && !o.MerchantId.StartsWith("TU_", StringComparison.OrdinalIgnoreCase),
                "Niubiz:MerchantId no configurado")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username) && !o.Username.StartsWith("TU_", StringComparison.OrdinalIgnoreCase),
                "Niubiz:Username no configurado")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Password) && !o.Password.StartsWith("TU_", StringComparison.OrdinalIgnoreCase),
                "Niubiz:Password no configurado")
            .Validate(o => Uri.IsWellFormedUriString(o.BaseUrl, UriKind.Absolute), "Niubiz:BaseUrl inválido")
            .ValidateOnStart();

        services.AddHttpClient<INiubizGateway, NiubizClient>((sp, http) =>
        {
            var opts = sp.GetRequiredService<IOptions<NiubizOptions>>().Value;
            http.BaseAddress = new Uri(opts.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(30);
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}
