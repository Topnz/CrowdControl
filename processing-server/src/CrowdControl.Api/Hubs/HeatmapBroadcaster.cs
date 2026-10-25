using CrowdControl.Application;
using CrowdControl.Shared;
using Microsoft.AspNetCore.SignalR;

namespace CrowdControl.Api.Hubs;

public class HeatmapBroadcaster : BackgroundService
{
    private readonly GridAggregator _aggregator;
    private readonly IHubContext<HeatmapHub> _hub;

    public HeatmapBroadcaster(GridAggregator aggregator, IHubContext<HeatmapHub> hub)
    {
        _aggregator = aggregator;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            //har sat den til 3 sekunder, fordi det er jeg talte da jeg prøvede P og T's cloudflare link
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

            HeatmapSnapshotDto snapshotDto = _aggregator.GetSnapshot();
            _aggregator.Reset();

            await _hub.Clients.All.SendAsync("HeatmapUpdated", snapshotDto, stoppingToken);
        }
    }
}
