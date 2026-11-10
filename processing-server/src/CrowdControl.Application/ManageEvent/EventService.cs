using CrowdControl.Domain;

namespace CrowdControl.Application.ManageEvent;

// Rule: ID is only valid after the event is saved
public class EventService(IEventRepository eventRepository)
{
    public async Task<Event> CreateAsync(string name, DateTime startTime, DateTime endTime, int sendInterval, IEnumerable<GeoPoint> geoPoints, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // An event that has already ended can never receive positions, so it is most likely a
        // typo. An event that has started but not ended is allowed (created while it is running).
        // The rule needs the current time, which the domain does not know, so it lives here.
        if (endTime <= DateTime.UtcNow)
        {
            throw new ArgumentException("End time must be in the future", nameof(endTime));
        }

        Boundary boundary = new Boundary(geoPoints);
        Event newEvent = new Event(name, startTime, endTime, sendInterval, boundary);
        await eventRepository.SaveAsync(newEvent, ct);
        
        return newEvent;
    }
    
    public Task<IReadOnlyList<Event>> GetActiveAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return eventRepository.GetActiveAsync(DateTime.UtcNow, ct);
    }
}