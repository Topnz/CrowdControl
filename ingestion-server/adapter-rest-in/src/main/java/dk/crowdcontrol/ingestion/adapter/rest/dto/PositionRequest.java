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
 * JSON-body i POST /positions fra mobil-appen.
 * Se PositionRequest i contracts/openapi/ingestion-api.yaml.
 * Tal er Double (ikke double), så et manglende felt bliver null og fanges af @NotNull.
 */
public record PositionRequest(
    @NotNull UUID sessionId,
    @NotNull UUID eventId,
    @NotNull @DecimalMin("-90") @DecimalMax("90") Double latitude,
    @NotNull @DecimalMin("-180") @DecimalMax("180") Double longitude,
    @NotNull @PositiveOrZero Double accuracy,
    @NotNull Instant timestamp)
{
  /** eventId er ikke en del af Position, så den sendes separat til use casen. */
  public Position toDomain()
  {
    return new Position(new SessionId(sessionId), new GeoPoint(latitude, longitude), accuracy, timestamp);
  }
}
