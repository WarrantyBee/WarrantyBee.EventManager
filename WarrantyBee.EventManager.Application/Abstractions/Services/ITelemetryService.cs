using WarrantyBee.EventManager.Domain.Enums;

namespace WarrantyBee.EventManager.Application.Abstractions.Services;

/// <summary>
/// Defines a service for tracking events, metrics, and logs for telemetry purposes.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Tracks a custom event with optional properties.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="properties">Optional properties associated with the event.</param>
    void TrackEvent(string eventName, IDictionary<string, object>? properties = null);

    /// <summary>
    /// Logs a message with a specified log level and optional context.
    /// </summary>
    /// <param name="level">The severity level of the log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Optional context data for the log.</param>
    void Log(LogLevel level, string message, IDictionary<string, object>? context = null);

    /// <summary>
    /// Logs an exception with a specified log level and optional context.
    /// </summary>
    /// <param name="level">The severity level of the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="context">Optional context data for the log.</param>
    void Log(LogLevel level, Exception exception, IDictionary<string, object>? context = null);

    /// <summary>
    /// Tracks a numeric metric with optional properties.
    /// </summary>
    /// <param name="metricName">The name of the metric.</param>
    /// <param name="value">The value of the metric.</param>
    /// <param name="properties">Optional properties associated with the metric.</param>
    void TrackMetric(string metricName, double value, IDictionary<string, object>? properties = null);

    /// <summary>
    /// Flushes any buffered telemetry data.
    /// </summary>
    void Flush();
}
