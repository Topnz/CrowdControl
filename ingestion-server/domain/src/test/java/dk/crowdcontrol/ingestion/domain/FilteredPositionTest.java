package dk.crowdcontrol.ingestion.domain;

import org.junit.jupiter.api.Test;

import java.time.Duration;
import java.time.Instant;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Tests the domain rule in {@link FilteredPosition#from(Event, Position)}: area, time span
 * and zones, including overlapping zones.
 */
class FilteredPositionTest
{
  private static final Instant START = Instant.parse("2026-07-01T10:00:00Z");
  private static final Instant END = Instant.parse("2026-07-01T22:00:00Z");
  private static final Instant DURING = Instant.parse("2026-07-01T12:00:00Z");

  private final Zone zoneA = new Zone(UUID.randomUUID(), "A", square(0, 0, 1));
  private final Zone zoneB = new Zone(UUID.randomUUID(), "B", square(0.5, 0.5, 1));
  private final Event event = new Event(UUID.randomUUID(), "Festival", START, END,
      Duration.ofSeconds(30), square(0, 0, 4), List.of(zoneA, zoneB));

  @Test
  void positionOutsideEventAreaIsRejected()
  {
    assertTrue(FilteredPosition.from(event, positionAt(5, 5, DURING)).isEmpty());
  }

  @Test
  void positionOutsideEventTimeIsRejected()
  {
    assertTrue(FilteredPosition.from(event, positionAt(0.2, 0.2, END)).isEmpty());
  }

  @Test
  void positionInEventButOutsideZonesHasNoZones()
  {
    Optional<FilteredPosition> result = FilteredPosition.from(event, positionAt(3, 3, DURING));

    assertTrue(result.isPresent());
    assertEquals(event.getId(), result.get().getEventId());
    assertEquals(List.of(), result.get().getZoneIds());
  }

  @Test
  void positionInOverlappingZonesHasBothZones()
  {
    Optional<FilteredPosition> result = FilteredPosition.from(event, positionAt(0.75, 0.75, DURING));

    assertEquals(List.of(zoneA.getId(), zoneB.getId()), result.orElseThrow().getZoneIds());
  }

  @Test
  void sessionIdIsKeptOnFilteredPosition()
  {
    Position position = positionAt(0.2, 0.2, DURING);

    FilteredPosition result = FilteredPosition.from(event, position).orElseThrow();

    assertEquals(position.getSessionId(), result.getPosition().getSessionId());
    assertEquals(List.of(zoneA.getId()), result.getZoneIds());
  }

  /** Position with a random session ID and 5 metres accuracy. */
  private static Position positionAt(double lat, double lon, Instant time)
  {
    return new Position(new SessionId(UUID.randomUUID()), new GeoPoint(lat, lon), 5, time);
  }

  /** Square with its south-west corner at (lat, lon) and sides of size degrees. */
  private static Boundary square(double lat, double lon, double size)
  {
    return new Boundary(List.of(
        new GeoPoint(lat, lon),
        new GeoPoint(lat, lon + size),
        new GeoPoint(lat + size, lon + size),
        new GeoPoint(lat + size, lon)));
  }
}
