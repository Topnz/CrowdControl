package dk.crowdcontrol.ingestion.adapter.rabbit.out.dto;

import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import dk.crowdcontrol.ingestion.domain.Position;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

/**
 * Message on the positions queue (Java -> C#).
 * The field names are the JSON keys and must match contracts/schemas/rabbitMQ/PositionReceived.v1.json.
 *
 * @param sessionId the device's pseudonymous session ID
 * @param eventId   the event the position belongs to
 * @param zoneIds   the zones the position lies in; empty if it lies outside all zones
 * @param latitude  latitude in degrees
 * @param longitude longitude in degrees
 * @param accuracy  accuracy in metres
 * @param timestamp when the position was measured; sent as ISO 8601 text
 */
public record PosData(
    UUID sessionId,
    UUID eventId,
    List<UUID> zoneIds,
    double latitude,
    double longitude,
    double accuracy,
    Instant timestamp)
{
  /**
   * @param filteredPosition the accepted position
   * @return the message to send to the positions queue
   */
  public static PosData from(FilteredPosition filteredPosition)
  {
    Position position = filteredPosition.getPosition();
    return new PosData(
        position.getSessionId().getValue(),
        filteredPosition.getEventId(),
        filteredPosition.getZoneIds(),
        position.getGeoPoint().getLatitude(),
        position.getGeoPoint().getLongitude(),
        position.getAccuracy(),
        position.getTimestamp());
  }
}
