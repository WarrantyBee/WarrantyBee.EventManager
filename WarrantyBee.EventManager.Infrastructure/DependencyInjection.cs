using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WarrantyBee.EventManager.Application.Abstractions.Persistence;
using WarrantyBee.EventManager.Application.Abstractions.Services;
using WarrantyBee.EventManager.Infrastructure.Persistence;
using WarrantyBee.EventManager.Infrastructure.Services;
using WarrantyBee.EventManager.Infrastructure.Background;

namespace WarrantyBee.EventManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("WebhookClient", client => {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        
        services.AddSingleton<IEventStreamService, UpstashStreamService>();
        services.AddScoped<IWebhookService, WebhookService>();

        // High-scale Background processing
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<EventProcessorHostedService>();

        return services;
    }
}
