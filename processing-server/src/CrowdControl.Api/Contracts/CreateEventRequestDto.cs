using System.ComponentModel.DataAnnotations;

namespace CrowdControl.Api.Contracts;

public class CreateEventRequestDto
{
    // [Required] on a string also rejects "" and whitespace, so no [MinLength(1)] is needed
    [Required]
    public required string Name { get; set; }
    // DateTimeOffset instead of DateTime, so the offset the client sends ("Z" or "+01:00") is kept
    // and .UtcDateTime converts it to UTC. The contract's date-time format requires an offset;
    // System.Text.Json still accepts a time without one and then assumes the server's offset.
    public required DateTimeOffset StartTime { get; set; }
    public required DateTimeOffset EndTime { get; set; }
    // Same rule as "minimum": 1 in the contract and as the domain's check, but with the key
    // "SendIntervalSeconds" in the 400 instead of the domain's parameter name
    [Range(1, int.MaxValue)]
    public required int SendIntervalSeconds { get; set; }
    [Required, MinLength(3)] 
    public required List<GeoPointDto> Area { get; set; }
}