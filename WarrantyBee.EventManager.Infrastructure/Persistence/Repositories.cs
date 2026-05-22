using System.Data;
using Dapper;
using WarrantyBee.EventManager.Application.Abstractions.Persistence;
using WarrantyBee.EventManager.Domain.Entities;

namespace WarrantyBee.EventManager.Infrastructure.Persistence;

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
        var parameters = new DynamicParameters();
        parameters.Add("@in_event_type", eventLog.EventType);
        parameters.Add("@in_payload", eventLog.Payload);
        parameters.Add("@in_status", eventLog.Status);

        // Simple insert for now, assuming usp_LogEvent or similar exists
        // Actually, we created tblEventLogs with usp_CreateTable which adds audit columns.
        // We'll use a direct INSERT for performance at scale, or create a specific proc.
        // Given the scale, direct Dapper with parameterized SQL is often faster than sprocs for simple inserts.
        var sql = @"INSERT INTO tblEventLogs (internal_id, event_type, payload, status, created_at, void) 
                    VALUES (NEWID(), @in_event_type, @in_payload, @in_status, GETUTCDATE(), 0);
                    SELECT CAST(SCOPE_IDENTITY() as BIGINT);";
        
        return await connection.ExecuteScalarAsync<long>(sql, parameters);
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
        return await connection.QueryAsync<EventSubscription>(
            "SELECT * FROM tblEventSubscriptions WHERE event_type = @eventType AND is_active = 1 AND void = 0", 
            new { eventType });
    }
}
