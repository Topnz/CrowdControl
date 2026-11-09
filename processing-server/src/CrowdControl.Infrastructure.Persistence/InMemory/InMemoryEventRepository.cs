using System.Collections.Concurrent;
using CrowdControl.Application.ManageEvent;
using CrowdControl.Domain;

namespace CrowdControl.Infrastructure.Persistence.InMemory;

public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();
    
    public Task SaveAsync(Event ev, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        ev.AssignId(Guid.CreateVersion7());
        
        return _events.TryAdd(ev.Id, ev)
            ? Task.CompletedTask
            : throw new InvalidOperationException($"Event {ev.Id} already exists.");
    }

    public Task<IReadOnlyList<Event>> GetActiveAsync(DateTime now, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IReadOnlyList<Event> active = _events.Values
            .Where(ev => ev.EndTime > now)
            .ToList();
        
        return Task.FromResult(active);
    }
}