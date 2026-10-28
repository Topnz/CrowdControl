namespace CrowdControl.Application.MonitorEvent;

/// <summary>
/// Number of positions counted inside one zone.
/// </summary>
public class ZoneCount
{
    public string Name { get; set; } = "";

    // Null when the zone has no capacity
    public int? Capacity { get; set; }

    public int Count { get; set; }
}
