using System.Text.Json;
using StackExchange.Redis;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Infrastructure.Services;

public class UpstashStreamService : IEventStreamService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public UpstashStreamService()
    {
        var host = Environment.GetEnvironmentVariable("WB__UPSTASH_HOST") ?? "localhost";
        var token = Environment.GetEnvironmentVariable("WB__UPSTASH_TOKEN") ?? "";
        
        // Clean host for StackExchange.Redis (remove https://)
        var cleanHost = host.Replace("https://", "").Replace("http://", "");
        
        var options = new ConfigurationOptions
        {
            EndPoints = { $"{cleanHost}:6379" },
            Password = token,
            Ssl = true,
            AbortOnConnectFail = false
        };

        _redis = ConnectionMultiplexer.Connect(options);
        _db = _redis.GetDatabase();
    }

    public async Task PublishAsync<T>(string streamName, T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        await _db.StreamAddAsync(streamName, new NameValueEntry[] 
        { 
            new NameValueEntry("data", json),
            new NameValueEntry("type", typeof(T).Name)
        });
    }

    public async Task<IEnumerable<T>> ConsumeAsync<T>(string streamName, string consumerGroup, string consumerName, int count = 10)
    {
        // Ensure consumer group exists
        try
        {
            await _db.StreamCreateConsumerGroupAsync(streamName, consumerGroup, "0-0", true);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("already exists"))
        {
            // Group already exists, ignore
        }

        var entries = await _db.StreamReadGroupAsync(streamName, consumerGroup, consumerName, ">", count);
        var results = new List<T>();

        foreach (var entry in entries)
        {
            var json = entry.Values.FirstOrDefault(v => v.Name == "data").Value;
            if (!json.IsNull)
            {
                var item = JsonSerializer.Deserialize<T>(json!);
                if (item != null) results.Add(item);
            }
            
            // Acknowledge the message
            await _db.StreamAcknowledgeAsync(streamName, consumerGroup, entry.Id);
        }

        return results;
    }
}
