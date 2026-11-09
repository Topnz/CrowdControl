namespace CrowdControl.Api.Contracts;

public class ZoneDto
{
    public required Guid ZoneId { get; set; }
    public required string Name { get; set; }
    public required List<GeoPointDto> Area { get; set; }
}