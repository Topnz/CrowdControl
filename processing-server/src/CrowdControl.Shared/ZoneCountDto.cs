namespace CrowdControl.Shared;

public class ZoneCountDto
{
    public string Name { get; set; }
    public int? Capacity { get; set; } // null when the zone has no capacity
    public int Count { get; set; }
    
}