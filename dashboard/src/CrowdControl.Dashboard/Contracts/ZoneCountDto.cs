namespace CrowdControl.Dashboard.Contracts;

public class ZoneCountDto
{
    public string Name { get; set; } = "";

    // Null when the zone has no capacity
    public int? Capacity { get; set; }

    public int Count { get; set; }
}
