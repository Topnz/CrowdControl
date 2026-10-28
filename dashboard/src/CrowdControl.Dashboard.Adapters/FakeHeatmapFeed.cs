using CrowdControl.Dashboard.Core.Contracts;
using CrowdControl.Dashboard.Core.Ports;

namespace CrowdControl.Dashboard.Adapters;

/// <summary>
/// Sends a random snapshot every 3 seconds (like HeatmapBroadcaster in the Api), so the dashboard
/// can run without the Api. The zones cover the cases the UI must handle: no capacity, close to
/// 90 % of capacity, and over capacity.
/// </summary>
public class FakeHeatmapFeed : IHeatmapFeed
{
    // Grid origin near the map's start position in map.js, and the Api's default cell size
    private const double MinLat = 55.6741;
    private const double MinLon = 12.5653;
    private const double CellSizeLat = 0.0002;
    private const double CellSizeLon = 0.00035;
    private const int GridSize = 20;

    private readonly TimeSpan _interval;
    private readonly Random _random;
    private CancellationTokenSource? _cancellationSource;
    private Task? _loop;

    public event Func<HeatmapSnapshotDto, Task>? SnapshotReceived;

    public FakeHeatmapFeed() : this(TimeSpan.FromSeconds(3), new Random())
    {
    }

    // Tests pass a short interval and a seeded Random
    public FakeHeatmapFeed(TimeSpan interval, Random random)
    {
        _interval = interval;
        _random = random;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = RunAsync(_cancellationSource.Token);
        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_interval);
        try
        {
            do
            {
                Func<HeatmapSnapshotDto, Task>? handler = SnapshotReceived;
                if (handler != null)
                {
                    await handler(NextSnapshot());
                }
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }
        catch (OperationCanceledException)
        {
            // Stopped by DisposeAsync
        }
    }

    public HeatmapSnapshotDto NextSnapshot()
    {
        HeatmapSnapshotDto snapshot = new HeatmapSnapshotDto
        {
            EventName = "Fake event",
            Timestamp = DateTime.UtcNow,
            MinLat = MinLat,
            MinLon = MinLon,
            CellSizeLat = CellSizeLat,
            CellSizeLon = CellSizeLon
        };

        // Cells cluster around the middle of the grid; only cells with positions are sent
        for (int column = 0; column < GridSize; column++)
        {
            for (int row = 0; row < GridSize; row++)
            {
                double distance = Math.Sqrt(Math.Pow(column - GridSize / 2.0, 2) + Math.Pow(row - GridSize / 2.0, 2));
                int count = (int)(_random.Next(0, 12) - distance);
                if (count > 0)
                {
                    snapshot.Cells.Add(new CellDto { X = column, Y = row, Count = count });
                }
            }
        }

        snapshot.Zones.Add(new ZoneCountDto { Name = "Bøgescenen Sektion A", Capacity = 100, Count = _random.Next(90, 101) });
        snapshot.Zones.Add(new ZoneCountDto { Name = "Bøgescenen Sektion B", Capacity = 50, Count = _random.Next(51, 60) });
        snapshot.Zones.Add(new ZoneCountDto { Name = "Madområdet", Capacity = null, Count = _random.Next(0, 40) });

        return snapshot;
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationSource != null)
        {
            await _cancellationSource.CancelAsync();
            if (_loop != null)
            {
                await _loop;
            }
            _cancellationSource.Dispose();
            _cancellationSource = null;
            _loop = null;
        }
    }
}
