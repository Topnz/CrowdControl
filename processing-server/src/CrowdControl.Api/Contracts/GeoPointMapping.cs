using CrowdControl.Domain;

namespace CrowdControl.Api.Contracts;

public static class GeoPointMapping
{
    public static GeoPoint ToDomain(this GeoPointDto geoPointDto)
    {
        GeoPoint geoPoint = new GeoPoint
        {
            Latitude = geoPointDto.Latitude,
            Longitude = geoPointDto.Longitude
        };
        
        return geoPoint;
    }
    
    public static GeoPointDto ToDto(this GeoPoint geoPoint)
    {
        GeoPointDto geoPointDto = new GeoPointDto
        {
            Latitude = geoPoint.Latitude,
            Longitude = geoPoint.Longitude
        };
        
        return geoPointDto;
    }
}