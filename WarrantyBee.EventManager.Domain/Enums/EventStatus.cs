namespace WarrantyBee.EventManager.Domain.Enums;

/// <summary>
/// Represents the processing status of an event.
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Event has been ingested and is waiting to be processed.
    /// </summary>
    Pending,

    /// <summary>
    /// Event is currently being processed by a worker.
    /// </summary>
    Processing,

    /// <summary>
    /// Event has been successfully processed and delivered to all subscribers.
    /// </summary>
    Completed,

    /// <summary>
    /// Event processing failed after all retries.
    /// </summary>
    Failed,

    /// <summary>
    /// Event is partially successful (some deliveries failed).
    /// </summary>
    PartialSuccess
}
