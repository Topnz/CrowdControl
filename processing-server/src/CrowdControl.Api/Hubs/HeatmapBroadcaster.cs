using CrowdControl.Application;
using CrowdControl.Shared;
using Microsoft.AspNetCore.SignalR;

namespace CrowdControl.Api.Hubs;

public class HeatmapBroadcaster : BackgroundService
{
    private readonly GridAggregator _aggregator;
    private readonly IHubContext<HeatmapHub> _hub;
    private readonly LatestHeatmap _latest;

    public HeatmapBroadcaster(GridAggregator aggregator, IHubContext<HeatmapHub> hub, LatestHeatmap latest)
    {
        _aggregator = aggregator;
        _hub = hub;
        _latest = latest;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            // Set to 3 seconds, because that is what I counted when I tried P and T's Cloudflare link
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

            HeatmapSnapshotDto snapshotDto = _aggregator.GetSnapshot();
            // Disabled: the cells accumulate over time instead of being reset every 3 seconds
            // _aggregator.Reset();
            _latest.Snapshot = snapshotDto;

            await _hub.Clients.All.SendAsync("HeatmapUpdated", snapshotDto, stoppingToken);
        }
    }
}
