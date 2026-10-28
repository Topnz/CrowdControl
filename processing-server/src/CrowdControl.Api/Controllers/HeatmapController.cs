using CrowdControl.Api.Contracts;
using CrowdControl.Api.Hubs;
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
        // Intet snapshot før broadcasteren har kørt første gang (efter 3 sekunder)
        if (_latest.Snapshot == null)
        {
            return NoContent();
        }

        return _latest.Snapshot;
    }
}
