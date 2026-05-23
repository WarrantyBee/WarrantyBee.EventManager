using Microsoft.AspNetCore.Mvc;
using WarrantyBee.EventManager.Application.Abstractions.Services;
using WarrantyBee.EventManager.Application.Contracts.Events;
using WarrantyBee.EventManager.Api.Filters;

namespace WarrantyBee.EventManager.Api.Controllers;

[ApiController]
[Route("api/events")]
[ApiKey]
public class EventsController : ControllerBase
{
    private readonly IEventStreamService _streamService;
    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventsController"/> class.
    /// </summary>
    /// <param name="streamService">The service used for interacting with the event stream.</param>
    /// <param name="telemetry">The telemetry service for tracking metrics.</param>
    public EventsController(IEventStreamService streamService, ITelemetryService telemetry)
    {
        _streamService = streamService;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Ingests a new event and pushes it to the high-throughput stream for processing.
    /// </summary>
    /// <param name="evt">The incoming event payload.</param>
    /// <returns>202 Accepted if the event was successfully queued.</returns>
    [HttpPost]
    public async Task<IActionResult> Ingest([FromBody] IncomingEvent evt)
    {
        // Validation (simplified)
        if (string.IsNullOrWhiteSpace(evt.Type) || string.IsNullOrWhiteSpace(evt.Data))
        {
            return BadRequest("Event Type and Data are required.");
        }

        _telemetry.Log(Domain.Enums.LogLevel.Info, $"Ingesting event: {evt.Type}");

        // Push directly to Redis Stream (The shock-absorber)
        await _streamService.PublishAsync("main_event_stream", evt);

        _telemetry.TrackEvent("EventIngested", new Dictionary<string, object> { ["EventType"] = evt.Type });

        // Immediately return 202 to the caller to free up the request thread
        return Accepted();
    }
}
