using CrowdControl.Api.Hubs;
using CrowdControl.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CrowdControl.Api.Controllers;

[ApiController]
[Route("heatmap")]
public class HeatmapController : ControllerBase
{
    private readonly LatestHeatmap _latest;

    public HeatmapController(LatestHeatmap latest)
    {
        _latest = latest;
    }

    [HttpGet]
    public ActionResult<HeatmapSnapshotDto> GetSnapshot()
    {
        // No snapshot until the broadcaster has run for the first time (after 3 seconds)
        if (_latest.Snapshot == null)
        {
            return NoContent();
        }

        return _latest.Snapshot;
    }
}
