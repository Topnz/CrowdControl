using CrowdControl.Dashboard.Core.Contracts;

namespace CrowdControl.Dashboard.Core.Ports;

/// <summary>
/// Live heatmap snapshots for the dashboard. The UI depends on this port only, so the data can
/// come from processing-server via SignalR or from a fake when running offline.
/// </summary>
public interface IHeatmapFeed : IAsyncDisposable
{
    /// <summary>Raised for every new snapshot. Handlers are awaited, so they can call JS interop.</summary>
    event Func<HeatmapSnapshotDto, Task>? SnapshotReceived;

    /// <summary>Starts delivering snapshots.</summary>
    Task StartAsync(CancellationToken cancellationToken = default);
}
