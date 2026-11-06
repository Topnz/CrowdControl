using CrowdControl.Application.MonitorEvent;

namespace CrowdControl.Application;

/// <summary>
/// Adds positions from ingestion-server to the heatmap and the zone counts.
/// Imprecise positions are filtered out first (ID-40).
/// </summary>
public class ReceivePositionService : IReceivePositionUseCase
{
    /// <summary>
    /// Positions with an accuracy above this many metres are not counted (ID-40).
    /// At 50 m the uncertainty is about two grid cells (about 22 m each), so a less
    /// precise position could just as well belong to a neighbouring cell or zone.
    /// </summary>
    public const double MaxAccuracyMeters = 50;

    private readonly GridAggregator _aggregator;

    public ReceivePositionService(GridAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public Task HandleAsync(PositionReceived position, CancellationToken ct)
    {
        if (position.Accuracy > MaxAccuracyMeters)
            return Task.CompletedTask;

        _aggregator.Add(position.Latitude, position.Longitude);
        return Task.CompletedTask;
    }
}
