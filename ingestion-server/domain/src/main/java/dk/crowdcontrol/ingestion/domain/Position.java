package dk.crowdcontrol.ingestion.domain;

import java.time.Instant;
import java.util.Objects;

/**
 * A position received from the mobile app. Contains only a pseudonymous sessionId,
 * no information about the person.
 */
public class Position
{
  private final SessionId sessionId;
  private final GeoPoint geoPoint;
  private final double accuracy;
  private final Instant timestamp;

  /**
   * @param sessionId the device's pseudonymous session ID
   * @param geoPoint  where the device is
   * @param accuracy  accuracy in metres (ID-40); must not be negative
   * @param timestamp when the position was measured on the device
   * @throws IllegalArgumentException if accuracy is negative
   * @throws NullPointerException     if sessionId, geoPoint or timestamp is null
   */
  public Position(SessionId sessionId, GeoPoint geoPoint, double accuracy, Instant timestamp)
  {
    if (accuracy < 0)
      throw new IllegalArgumentException("accuracy kan ikke være negativ");
    this.sessionId = Objects.requireNonNull(sessionId, "sessionId");
    this.geoPoint = Objects.requireNonNull(geoPoint, "geoPoint");
    this.accuracy = accuracy;
    this.timestamp = Objects.requireNonNull(timestamp, "timestamp");
  }

  /** @return the device's pseudonymous session ID */
  public SessionId getSessionId()
  {
    return sessionId;
  }

  /** @return where the device is */
  public GeoPoint getGeoPoint()
  {
    return geoPoint;
  }

  /** @return accuracy in metres, as reported by the Geolocation API */
  public double getAccuracy()
  {
    return accuracy;
  }

  /** @return when the position was measured on the device */
  public Instant getTimestamp()
  {
    return timestamp;
  }
}
