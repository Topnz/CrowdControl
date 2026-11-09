namespace CrowdControl.Api.Contracts;

public class EventDto
{
    public required Guid EventId { get; init; }
    public required string Name { get; init; }
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public required int SendIntervalSeconds { get; init; }
    public required List<GeoPointDto> Area { get; init; }
    public required List<ZoneDto> Zones { get; init; }
}