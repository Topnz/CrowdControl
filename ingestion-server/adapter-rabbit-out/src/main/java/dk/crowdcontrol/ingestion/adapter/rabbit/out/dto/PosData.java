package dk.crowdcontrol.ingestion.adapter.rabbit.out.dto;

import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import dk.crowdcontrol.ingestion.domain.Position;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

/**
 * Besked på positions-køen (Java -> C#).
 * Feltnavnene er JSON-nøglerne og skal matche contracts/schemas/rabbitMQ/PositionReceived.v1.json.
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
