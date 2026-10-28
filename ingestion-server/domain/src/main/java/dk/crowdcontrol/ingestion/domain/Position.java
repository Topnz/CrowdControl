package dk.crowdcontrol.ingestion.domain;

import java.time.Instant;
import java.util.Objects;

/**
 * En position modtaget fra mobil-appen. Indeholder kun et pseudonymt sessionId,
 * ingen oplysninger om personen.
 */
public class Position
{
  private final SessionId sessionId;
  private final GeoPoint geoPoint;
  private final double accuracy;
  private final Instant timestamp;

  public Position(SessionId sessionId, GeoPoint geoPoint, double accuracy, Instant timestamp)
  {
    if (accuracy < 0)
      throw new IllegalArgumentException("accuracy kan ikke være negativ");
    this.sessionId = Objects.requireNonNull(sessionId, "sessionId");
    this.geoPoint = Objects.requireNonNull(geoPoint, "geoPoint");
    this.accuracy = accuracy;
    this.timestamp = Objects.requireNonNull(timestamp, "timestamp");
  }

  public SessionId getSessionId()
  {
    return sessionId;
  }

  public GeoPoint getGeoPoint()
  {
    return geoPoint;
  }

  /** Nøjagtighed i meter, som Geolocation API'et angiver den. */
  public double getAccuracy()
  {
    return accuracy;
  }

  public Instant getTimestamp()
  {
    return timestamp;
  }
}
