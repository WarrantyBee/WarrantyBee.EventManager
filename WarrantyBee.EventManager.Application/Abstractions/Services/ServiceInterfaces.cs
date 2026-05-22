namespace WarrantyBee.EventManager.Application.Abstractions.Services;

public interface IEventStreamService
{
    Task PublishAsync<T>(string streamName, T payload);
    Task<IEnumerable<T>> ConsumeAsync<T>(string streamName, string consumerGroup, string consumerName, int count = 10);
}

public interface IWebhookService
{
    Task<bool> SendWebhookAsync(string url, string payload, string secret);
}
