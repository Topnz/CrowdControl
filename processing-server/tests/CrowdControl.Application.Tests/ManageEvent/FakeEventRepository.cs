using CrowdControl.Application.ManageEvent;
using CrowdControl.Domain;

namespace CrowdControl.Application.Tests.ManageEvent;

/// <summary>
/// Test fake of IEventRepository. Application.Tests must not reference Persistence, so the
/// in-memory repository from there is not used. Like the real repositories, it assigns the ID
/// when an event is saved.
/// </summary>
internal sealed class FakeEventRepository : IEventRepository
{
    public List<Event> Saved { get; } = [];

    public Task SaveAsync(Event eventToSave, CancellationToken ct)
    {
        eventToSave.AssignId(Guid.NewGuid());
        Saved.Add(eventToSave);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Event>> GetActiveAsync(DateTime now, CancellationToken ct)
    {
        IReadOnlyList<Event> active = Saved.Where(storedEvent => storedEvent.EndTime > now).ToList();
        return Task.FromResult(active);
    }
}
