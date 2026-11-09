using CrowdControl.Api.Contracts;
using CrowdControl.Application.ManageEvent;
using CrowdControl.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CrowdControl.Api.Controllers;

/// <summary>
/// Creates events (ID-13) and lists the events that have not ended yet, which ingestion-server
/// fetches at startup. Follows POST /events and GET /events/active in
/// contracts/openapi/processing-api.yaml.
/// </summary>
[ApiController]
[Route("events")]
public class EventsController(EventService eventService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent(CreateEventRequestDto request, CancellationToken ct)
    {
        // The attributes on the DTO are checked by [ApiController] before this method runs.
        // The rules that need the domain (end after start, end in the future, valid points)
        // throw ArgumentException, which becomes a 400 here instead of a 500.
        try
        {
            Event createdEvent = await eventService.CreateAsync(
                request.Name,
                request.StartTime.UtcDateTime,
                request.EndTime.UtcDateTime,
                request.SendIntervalSeconds,
                request.Area.Select(point => point.ToDomain()),
                ct);

            return Created($"/events/{createdEvent.Id}", createdEvent.ToDto());
        }
        catch (ArgumentException exception)
        {
            ModelStateDictionary errors = new ModelStateDictionary();
            errors.AddModelError(exception.ParamName ?? string.Empty, exception.Message);
            return ValidationProblem(errors);
        }
    }

    // Upcoming and running events; ingestion-server must know an event before it starts
    [HttpGet("active")]
    public async Task<ActionResult<List<EventDto>>> GetActiveEvents(CancellationToken ct)
    {
        IReadOnlyList<Event> activeEvents = await eventService.GetActiveAsync(ct);

        return Ok(activeEvents.Select(activeEvent => activeEvent.ToDto()).ToList());
    }
}
