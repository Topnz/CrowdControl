namespace CrowdControl.Api.Contracts;

public class ZoneCountDto
{
    public string Name { get; set; } = "";

    // Null when the zone has no capacity (sent as JSON null)
    public int? Capacity { get; set; }

    public int Count { get; set; }
}
