using CrowdControl.Dashboard.Core.Contracts;

namespace CrowdControl.Dashboard.Core.Ports;

public interface ICreateEventFeed
{
    /// <summary>Sents a new event</summary>
    public void CreateEvent(CreateEventRequestDto createEventRequestDto, CancellationToken cancellationToken);
}
