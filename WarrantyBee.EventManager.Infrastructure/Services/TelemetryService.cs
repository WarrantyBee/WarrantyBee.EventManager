using System.Net;
using Microsoft.Extensions.Logging;
using RestSharp;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Infrastructure.Services;

/// <summary>
/// Provides telemetry and logging services for the Event Manager, integrating with Better Stack.
/// </summary>
public class TelemetryService : ITelemetryService
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly RestClient? _client;
    private readonly string? _accessToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryService"/> class.
    /// </summary>
    /// <param name="logger">The .NET logger instance.</param>
    /// <param name="taskQueue">The background task queue for non-blocking ingestion.</param>
    public TelemetryService(ILogger<TelemetryService> logger, IBackgroundTaskQueue taskQueue)
    {
        _logger = logger;
        _taskQueue = taskQueue;

        var host = Environment.GetEnvironmentVariable("WB__BETTERSTACK_HOST");
        _accessToken = Environment.GetEnvironmentVariable("WB__BETTERSTACK_TOKEN");

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(_accessToken))
        {
            var baseUrl = host.StartsWith("http") ? host : $"https://{host}";
            _client = new RestClient(baseUrl);
        }
    }

    public void TrackEvent(string eventName, IDictionary<string, object>? properties = null)
    {
        _logger.LogInformation("Event: {EventName}, Properties: {@Properties}", eventName, properties);
        
        _taskQueue.QueueBackgroundWorkItemAsync(token =>
        {
            SendToBetterStack(Domain.Enums.LogLevel.Info, $"Event: {eventName}", properties);
            return ValueTask.CompletedTask;
        });
    }

    public void Log(Domain.Enums.LogLevel level, string message, IDictionary<string, object>? context = null)
    {
        _logger.Log(MapLevel(level), "Message: {Message}, Context: {@Context}", message, context);
        
        _taskQueue.QueueBackgroundWorkItemAsync(token =>
        {
            SendToBetterStack(level, message, context);
            return ValueTask.CompletedTask;
        });
    }

    public void Log(Domain.Enums.LogLevel level, Exception exception, IDictionary<string, object>? context = null)
    {
        _logger.Log(MapLevel(level), exception, "Exception, Context: {@Context}", context);
        
        _taskQueue.QueueBackgroundWorkItemAsync(token =>
        {
            var extendedContext = context ?? new Dictionary<string, object>();
            extendedContext["Exception"] = exception.Message;
            extendedContext["StackTrace"] = exception.StackTrace ?? string.Empty;
            SendToBetterStack(level, exception.Message, extendedContext);
            return ValueTask.CompletedTask;
        });
    }

    public void TrackMetric(string metricName, double value, IDictionary<string, object>? properties = null)
    {
        _logger.LogInformation("Metric: {MetricName}, Value: {Value}, Properties: {@Properties}", metricName, value, properties);
        
        _taskQueue.QueueBackgroundWorkItemAsync(token =>
        {
            var context = properties ?? new Dictionary<string, object>();
            context["MetricValue"] = value;
            SendToBetterStack(Domain.Enums.LogLevel.Info, $"Metric: {metricName}", context);
            return ValueTask.CompletedTask;
        });
    }

    public void Flush() { }

    private Microsoft.Extensions.Logging.LogLevel MapLevel(Domain.Enums.LogLevel level) => level switch
    {
        Domain.Enums.LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
        Domain.Enums.LogLevel.Warn => Microsoft.Extensions.Logging.LogLevel.Warning,
        Domain.Enums.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
        Domain.Enums.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
        _ => Microsoft.Extensions.Logging.LogLevel.Information
    };

    private async void SendToBetterStack(Domain.Enums.LogLevel level, string message, IDictionary<string, object>? context = null)
    {
        if (_client == null || string.IsNullOrEmpty(_accessToken)) return;

        try
        {
            var request = new RestRequest("/", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            
            var payload = new
            {
                dt = DateTime.UtcNow.ToString("O"),
                level = level.ToString(),
                message = message,
                metadata = context
            };

            request.AddJsonBody(payload);
            await _client.ExecuteAsync(request);
        }
        catch
        {
            // Silent fail for telemetry
        }
    }
}
