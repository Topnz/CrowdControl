namespace CrowdControl.Application;

public class ReceivePositionService : IReceivePositionUseCase
{
    private readonly GridAggregator _aggregator;

    public ReceivePositionService(GridAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public Task HandleAsync(PositionReceived position, CancellationToken ct)
    {
        _aggregator.Add(position.Latitude, position.Longitude);
        return Task.CompletedTask;
    }
}
