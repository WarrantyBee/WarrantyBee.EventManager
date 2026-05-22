namespace WarrantyBee.EventManager.Application.Contracts.Events;

/// <summary>
/// Represents an event received by the ingestion API.
/// </summary>
public class IncomingEvent
{
    /// <summary>
    /// Gets or sets the type of the event (e.g., 'user.created').
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw JSON data associated with the event.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the event was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
