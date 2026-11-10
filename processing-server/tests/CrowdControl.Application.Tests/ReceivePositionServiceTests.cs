using CrowdControl.Application.MonitorEvent;

namespace CrowdControl.Application.Tests;

public class ReceivePositionServiceTests
{
    [Fact]
    public async Task HandleAsync_AccuracyAtThreshold_IsCounted()
    {
        GridAggregator aggregator = new GridAggregator("Test");
        ReceivePositionService service = new ReceivePositionService(aggregator);

        await service.HandleAsync(CreatePosition(ReceivePositionService.MaxAccuracyMeters), CancellationToken.None);

        GridCell cell = Assert.Single(aggregator.GetSnapshot().Cells);
        Assert.Equal(1, cell.Count);
    }

    [Fact]
    public async Task HandleAsync_AccuracyAboveThreshold_IsNotCounted()
    {
        GridAggregator aggregator = new GridAggregator("Test");
        ReceivePositionService service = new ReceivePositionService(aggregator);

        await service.HandleAsync(CreatePosition(ReceivePositionService.MaxAccuracyMeters + 0.1), CancellationToken.None);

        Assert.Empty(aggregator.GetSnapshot().Cells);
    }

    /// <summary>A position in Copenhagen with the given accuracy in metres.</summary>
    private static PositionReceived CreatePosition(double accuracy) =>
        new PositionReceived(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [],
            55.6761,
            12.5683,
            accuracy,
            DateTimeOffset.UtcNow);
}
