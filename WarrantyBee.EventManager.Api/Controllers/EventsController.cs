using Microsoft.AspNetCore.Mvc;
using WarrantyBee.EventManager.Application.Abstractions.Services;
using WarrantyBee.EventManager.Application.Contracts.Events;

namespace WarrantyBee.EventManager.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventStreamService _streamService;

    public EventsController(IEventStreamService streamService)
    {
        _streamService = streamService;
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

        // Push directly to Redis Stream (The shock-absorber)
        await _streamService.PublishAsync("main_event_stream", evt);

        // Immediately return 202 to the caller to free up the request thread
        return Accepted();
    }
}
