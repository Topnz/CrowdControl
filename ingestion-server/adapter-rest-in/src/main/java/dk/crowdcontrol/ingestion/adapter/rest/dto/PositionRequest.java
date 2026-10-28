package dk.crowdcontrol.ingestion.adapter.rest.dto;

import dk.crowdcontrol.ingestion.domain.GeoPoint;
import dk.crowdcontrol.ingestion.domain.Position;
import dk.crowdcontrol.ingestion.domain.SessionId;
import jakarta.validation.constraints.DecimalMax;
import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.PositiveOrZero;

import java.time.Instant;
import java.util.UUID;

/**
 * JSON body of POST /positions from the mobile app.
 * See PositionRequest in contracts/openapi/ingestion-api.yaml.
 * Numbers are Double (not double), so a missing field becomes null and is caught by @NotNull.
 *
 * @param sessionId the device's pseudonymous session ID
 * @param eventId   the event the mobile app is sending to (from ?event= in the link)
 * @param latitude  latitude between -90 and 90
 * @param longitude longitude between -180 and 180
 * @param accuracy  accuracy in metres; must not be negative
 * @param timestamp when the position was measured, as ISO 8601 text
 */
public record PositionRequest(
    @NotNull UUID sessionId,
    @NotNull UUID eventId,
    @NotNull @DecimalMin("-90") @DecimalMax("90") Double latitude,
    @NotNull @DecimalMin("-180") @DecimalMax("180") Double longitude,
    @NotNull @PositiveOrZero Double accuracy,
    @NotNull Instant timestamp)
{
  /**
   * eventId is not part of Position, so it is passed separately to the use case.
   *
   * @return the position as a domain object
   */
  public Position toDomain()
  {
    return new Position(new SessionId(sessionId), new GeoPoint(latitude, longitude), accuracy, timestamp);
  }
}
