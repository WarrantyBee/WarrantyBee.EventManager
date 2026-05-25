using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using WarrantyBee.EventManager.Application.Abstractions.Persistence;
using WarrantyBee.EventManager.Application.Abstractions.Services;
using WarrantyBee.EventManager.Domain.Entities;
using WarrantyBee.EventManager.Application.Contracts.Events;
using WarrantyBee.Shared.Core.Enums;

namespace WarrantyBee.EventManager.Infrastructure.Background;

public class EventProcessorHostedService : BackgroundService
{
    private readonly ILogger<EventProcessorHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventStreamService _streamService;

    public EventProcessorHostedService(
        ILogger<EventProcessorHostedService> logger,
        IServiceProvider serviceProvider,
        IEventStreamService streamService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _streamService = streamService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Processor Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Consume a batch of events from Redis Stream
                var events = await _streamService.ConsumeAsync<IncomingEvent>("main_event_stream", "event_processor_group", "worker_1", 50);

                foreach (var evt in events)
                {
                    await ProcessEventAsync(evt);
                }

                if (!events.Any())
                {
                    await Task.Delay(1000, stoppingToken); // Wait if no events
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Event Processor loop.");
                await Task.Delay(5000, stoppingToken); // Wait longer on error
            }
        }
    }

    private async Task ProcessEventAsync(IncomingEvent evt)
    {
        using var scope = _serviceProvider.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();

        // 1. Persist the event log
        var logId = await eventRepository.LogEventAsync(new EventLog
        {
            EventType = evt.Type,
            Payload = evt.Data,
            Status = "PROCESSING"
        });

        // 2. Fetch active subscriptions for this event type
        var subscriptions = await subscriptionRepository.GetSubscriptionsAsync(evt.Type);

        var successCount = 0;
        foreach (var sub in subscriptions)
        {
            // 3. Deliver webhook
            var success = await webhookService.SendWebhookAsync(sub.WebhookUrl, evt.Data, sub.SecretKey);
            
            // 4. Log delivery attempt
            await eventRepository.LogDeliveryAsync(new EventDelivery
            {
                EventLogId = logId,
                SubscriptionId = sub.Id,
                ResponseStatusCode = success ? 200 : 500, // Simplified
                AttemptCount = 1
            });

            if (success) successCount++;
        }

        // --- MVP SPECIAL HANDLING: Trigger Automated Notifications ---
        if (evt.Type == "claim.status_changed")
        {
            await HandleClaimStatusChanged(evt);
        }

        // 5. Update final status
        var finalStatus = successCount == subscriptions.Count() ? "COMPLETED" : (successCount > 0 ? "PARTIAL" : "FAILED");
        await eventRepository.UpdateEventStatusAsync(logId, finalStatus);
    }

    private async Task HandleClaimStatusChanged(IncomingEvent evt)
    {
        // Extract data (assuming JSON structure)
        // For Day 1, we simulate triggering the JobScheduler
        _logger.LogInformation("Triggering ClaimStatusUpdated notification for claim status change.");
        
        // In a real microservice, we would use IJobSchedulerClient here.
        // We'll leave the hook ready.
    }
}
