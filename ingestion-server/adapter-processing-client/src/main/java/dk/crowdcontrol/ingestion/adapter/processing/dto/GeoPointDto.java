package dk.crowdcontrol.ingestion.adapter.processing.dto;

/**
 * A point in an area from GET /events/active.
 * The field names are the JSON keys and must match GeoPoint in contracts/schemas/shared/common.v1.json.
 *
 * @param latitude  latitude in degrees (WGS84)
 * @param longitude longitude in degrees (WGS84)
 */
public record GeoPointDto(double latitude, double longitude)
{
}
