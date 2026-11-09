using System.ComponentModel.DataAnnotations;

namespace CrowdControl.Api.Contracts;

public class CreateEventRequestDto
{
    [Required, MinLength(1)]
    public required string Name { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required int SendIntervalSeconds { get; set; }
    [Required, MinLength(3)] 
    public required List<GeoPointDto> Area { get; set; }
}