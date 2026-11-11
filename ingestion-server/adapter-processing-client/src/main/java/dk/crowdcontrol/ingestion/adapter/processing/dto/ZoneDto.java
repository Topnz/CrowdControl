package dk.crowdcontrol.ingestion.adapter.processing.dto;

import java.util.List;
import java.util.UUID;

/**
 * A zone in an event from GET /events/active.
 * The field names are the JSON keys and must match Zone in contracts/schemas/shared/common.v1.json.
 *
 * @param zoneId the zone's ID from processing-server
 * @param name   the zone's name
 * @param area   the zone's corners in the order they were drawn (at least 3)
 */
public record ZoneDto(UUID zoneId, String name, List<GeoPointDto> area)
{
}
