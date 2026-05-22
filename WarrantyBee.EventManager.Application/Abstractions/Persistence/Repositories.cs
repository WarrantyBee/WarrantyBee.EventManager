using WarrantyBee.EventManager.Domain.Entities;

namespace WarrantyBee.EventManager.Application.Abstractions.Persistence;

public interface IEventRepository
{
    Task<long> LogEventAsync(EventLog eventLog);
    Task UpdateEventStatusAsync(long id, string status);
    Task LogDeliveryAsync(EventDelivery delivery);
}

public interface ISubscriptionRepository
{
    Task<IEnumerable<EventSubscription>> GetSubscriptionsAsync(string eventType);
}

public interface INotificationRepository
{
    Task CreateNotificationAsync(Notification notification);
}
