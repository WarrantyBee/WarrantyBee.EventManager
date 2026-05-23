using System.Data;
using Dapper;
using WarrantyBee.EventManager.Application.Abstractions.Persistence;
using WarrantyBee.EventManager.Domain.Entities;

namespace WarrantyBee.EventManager.Infrastructure.Persistence;

/// <summary>
/// Implementation of <see cref="IEventRepository"/> using Dapper.
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EventRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> LogEventAsync(EventLog eventLog)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO tblEventLogs (internal_id, event_type, payload, status, created_at, void) 
                    VALUES (NEWID(), @EventType, @Payload, @Status, GETUTCDATE(), 0);
                    SELECT CAST(SCOPE_IDENTITY() as BIGINT);";
        
        return await connection.ExecuteScalarAsync<long>(sql, eventLog);
    }

    public async Task UpdateEventStatusAsync(long id, string status)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("UPDATE tblEventLogs SET status = @status, updated_at = GETUTCDATE() WHERE id = @id", new { id, status });
    }

    public async Task LogDeliveryAsync(EventDelivery delivery)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO tblEventDeliveries (internal_id, event_log_id, subscription_id, response_status_code, response_body, attempt_count, created_at, void) 
                    VALUES (NEWID(), @EventLogId, @SubscriptionId, @ResponseStatusCode, @ResponseBody, @AttemptCount, GETUTCDATE(), 0)";
        await connection.ExecuteAsync(sql, delivery);
    }
}

/// <summary>
/// Implementation of <see cref="ISubscriptionRepository"/> using Dapper.
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SubscriptionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<EventSubscription>> GetSubscriptionsAsync(string eventType)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Use explicit aliases to map snake_case columns to PascalCase properties
        var sql = @"SELECT 
                        id AS Id, 
                        user_id AS UserId, 
                        event_type AS EventType, 
                        webhook_url AS WebhookUrl, 
                        secret_key AS SecretKey, 
                        is_active AS IsActive 
                    FROM tblEventSubscriptions 
                    WHERE event_type = @eventType AND is_active = 1 AND void = 0";
        
        return await connection.QueryAsync<EventSubscription>(sql, new { eventType });
    }
}
