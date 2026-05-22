namespace WarrantyBee.EventManager.Domain.Entities;

/// <summary>
/// Represents a logged event in the system.
/// </summary>
public class EventLog
{
    public long Id { get; set; }
    public Guid InternalId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents a webhook subscription for a specific event type.
/// </summary>
public class EventSubscription
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents an individual delivery attempt of an event to a subscriber.
/// </summary>
public class EventDelivery
{
    public long Id { get; set; }
    public long EventLogId { get; set; }
    public long SubscriptionId { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents an internal system notification for a user.
/// </summary>
public class Notification
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
