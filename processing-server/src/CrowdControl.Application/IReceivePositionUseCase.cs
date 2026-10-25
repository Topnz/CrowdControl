namespace CrowdControl.Application;

/// <summary>
/// In-port: kaldes af messaging-adapteren for hver position på positions-køen.
/// Beskeden ack'es først, når HandleAsync er færdig.
/// </summary>
public interface IReceivePositionUseCase
{
    Task HandleAsync(PositionReceived position, CancellationToken ct);
}
