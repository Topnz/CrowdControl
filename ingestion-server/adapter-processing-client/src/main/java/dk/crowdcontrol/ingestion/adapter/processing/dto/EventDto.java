package dk.crowdcontrol.ingestion.adapter.processing.dto;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

/**
 * An event from GET /events/active (processing-server -> ingestion-server).
 * The field names are the JSON keys and must match Event in contracts/schemas/shared/common.v1.json.
 *
 * @param eventId             the event's ID from processing-server
 * @param name                the event's name
 * @param startTime           start time; sent as ISO 8601 text in UTC
 * @param endTime             end time; sent as ISO 8601 text in UTC
 * @param sendIntervalSeconds how often the mobile app should send the position, in seconds
 * @param area                the event's corners in the order they were drawn (at least 3)
 * @param zones               the event's zones; empty if it has none
 */
public record EventDto(
    UUID eventId,
    String name,
    Instant startTime,
    Instant endTime,
    int sendIntervalSeconds,
    List<GeoPointDto> area,
    List<ZoneDto> zones)
{
}
