using WarrantyBee.Shared.Security;
using WarrantyBee.Shared.Security.Abstractions;
using WarrantyBee.Shared.Infrastructure;
using WarrantyBee.Shared.Infrastructure.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WarrantyBee.EventManager.Application.Abstractions.Persistence;
using WarrantyBee.EventManager.Application.Abstractions.Services;
using WarrantyBee.EventManager.Infrastructure.Persistence;
using WarrantyBee.EventManager.Infrastructure.Services;
using WarrantyBee.EventManager.Infrastructure.Background;
using WarrantyBee.Shared.Infrastructure.Persistence;

namespace WarrantyBee.EventManager.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services, including persistence, external service clients, and background workers.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("WebhookClient", client => {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Register Shared Infrastructure (Telemetry, Cache, Background Queue, Filters)
        services.AddWarrantyBeeInfrastructure();
        
        // Register Shared Security (ApiKeyService)
        services.AddWarrantyBeeSecurity();

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        
        services.AddSingleton<IEventStreamService, UpstashStreamService>();
        services.AddScoped<IWebhookService, WebhookService>();

        services.AddHostedService<EventProcessorHostedService>();

        return services;
    }
}