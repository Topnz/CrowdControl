using System.Collections.Concurrent;
using CrowdControl.Application.ManageEvent;
using CrowdControl.Domain;

namespace CrowdControl.Infrastructure.Persistence.InMemory;

public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();
    
    public Task SaveAsync(Event eventToSave, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        eventToSave.AssignId(Guid.CreateVersion7());
        
        return _events.TryAdd(eventToSave.Id, eventToSave)
            ? Task.CompletedTask
            : throw new InvalidOperationException($"Event {eventToSave.Id} already exists.");
    }

    public Task<IReadOnlyList<Event>> GetActiveAsync(DateTime now, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IReadOnlyList<Event> active = _events.Values
            .Where(storedEvent => storedEvent.EndTime > now)
            .ToList();
        
        return Task.FromResult(active);
    }
}