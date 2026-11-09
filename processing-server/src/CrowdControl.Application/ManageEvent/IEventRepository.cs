using CrowdControl.Domain;

namespace CrowdControl.Application.ManageEvent;

public interface IEventRepository
{
    Task SaveAsync(Event eventToSave, CancellationToken ct);
    Task<IReadOnlyList<Event>> GetActiveAsync(DateTime now, CancellationToken ct);
}