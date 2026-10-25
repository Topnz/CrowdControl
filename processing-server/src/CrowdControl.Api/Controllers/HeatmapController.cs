using CrowdControl.Application;
using CrowdControl.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CrowdControl.Api.Controllers;

[ApiController]
[Route("heatmap")]
public class HeatmapController : ControllerBase
{
    private readonly GridAggregator _aggregator;

    public HeatmapController(GridAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    [HttpGet]
    public HeatmapSnapshotDto GetSnapshot()
    {
        return _aggregator.GetSnapshot();
    }
}
