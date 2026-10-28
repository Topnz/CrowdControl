namespace CrowdControl.Application;

/// <summary>
/// In-port: called by the messaging adapter for each position on the positions queue.
/// The message is only acked once HandleAsync has finished.
/// </summary>
public interface IReceivePositionUseCase
{
    Task HandleAsync(PositionReceived position, CancellationToken ct);
}
