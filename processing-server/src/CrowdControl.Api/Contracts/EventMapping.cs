using CrowdControl.Domain;

namespace CrowdControl.Api.Contracts;

public static class EventMapping
{
    public static EventDto ToDto(this Event source)
    {
        EventDto eventDto = new EventDto
        {
            EventId = source.Id,
            Name = source.Name,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            SendIntervalSeconds = source.SendInterval,
            Area = source.Perimeter.GeoPoints
                .Select(geoPoint => geoPoint.ToDto())
                .ToList(),
            // TODO (ID-25): Implement zones
            Zones = []
        };
        
        return eventDto;
    }
}