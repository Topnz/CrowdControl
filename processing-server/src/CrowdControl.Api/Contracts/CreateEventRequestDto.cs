using System.ComponentModel.DataAnnotations;

namespace CrowdControl.Api.Contracts;

public class CreateEventRequestDto
{
    [Required, MinLength(1)]
    public required string Name { get; set; }
    // DateTimeOffset instead of DateTime, so the offset the client sends ("Z" or "+01:00") is kept
    // and .UtcDateTime converts it to UTC. The contract's date-time format requires an offset;
    // System.Text.Json still accepts a time without one and then assumes the server's offset.
    public required DateTimeOffset StartTime { get; set; }
    public required DateTimeOffset EndTime { get; set; }
    public required int SendIntervalSeconds { get; set; }
    [Required, MinLength(3)] 
    public required List<GeoPointDto> Area { get; set; }
}