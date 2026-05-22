namespace WarrantyBee.EventManager.Domain.Enums;

/// <summary>
/// Specifies the severity level of a log message.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning message indicating a potential issue.
    /// </summary>
    Warn,

    /// <summary>
    /// Error message indicating a failure.
    /// </summary>
    Error,

    /// <summary>
    /// Detailed debug information.
    /// </summary>
    Debug
}
