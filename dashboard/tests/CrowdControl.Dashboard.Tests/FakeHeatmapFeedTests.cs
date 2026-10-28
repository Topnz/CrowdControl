using CrowdControl.Dashboard.Adapters;
using CrowdControl.Dashboard.Core.Contracts;

namespace CrowdControl.Dashboard.Tests;

/// <summary>
/// The fake must send what the real Api sends, otherwise offline mode shows data that never
/// occurs in production.
/// </summary>
public class FakeHeatmapFeedTests
{
    [Fact]
    public void SnapshotsMatchContract()
    {
        FakeHeatmapFeed feed = new FakeHeatmapFeed(TimeSpan.FromSeconds(3), new Random(42));

        for (int i = 0; i < 20; i++)
        {
            HeatmapSnapshotContractTests.AssertMatchesContract(feed.NextSnapshot());
        }
    }

    [Fact]
    public void ZonesCoverNoCapacityNearCapacityAndOverCapacity()
    {
        FakeHeatmapFeed feed = new FakeHeatmapFeed(TimeSpan.FromSeconds(3), new Random(42));

        HeatmapSnapshotDto snapshot = feed.NextSnapshot();

        Assert.Contains(snapshot.Zones, zone => zone.Capacity == null);
        Assert.Contains(snapshot.Zones, zone => zone.Count >= 0.9 * zone.Capacity && zone.Count <= zone.Capacity);
        Assert.Contains(snapshot.Zones, zone => zone.Count > zone.Capacity);
        Assert.NotEmpty(snapshot.Cells);
    }

    [Fact]
    public async Task SendsSnapshotsUntilDisposed()
    {
        FakeHeatmapFeed feed = new FakeHeatmapFeed(TimeSpan.FromMilliseconds(10), new Random(42));
        int received = 0;
        TaskCompletionSource threeReceived = new TaskCompletionSource();
        feed.SnapshotReceived += _ =>
        {
            if (Interlocked.Increment(ref received) == 3)
            {
                threeReceived.TrySetResult();
            }
            return Task.CompletedTask;
        };

        await feed.StartAsync();
        await threeReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await feed.DisposeAsync();
        int afterDispose = received;
        await Task.Delay(50);

        Assert.Equal(afterDispose, received);
    }
}
