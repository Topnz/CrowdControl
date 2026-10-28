package dk.crowdcontrol.ingestion.application.service;

import dk.crowdcontrol.ingestion.application.port.in.OutsideActiveEventException;
import dk.crowdcontrol.ingestion.application.port.in.RateLimitExceededException;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.application.port.out.PositionPublisher;
import dk.crowdcontrol.ingestion.application.port.out.RateLimiter;
import dk.crowdcontrol.ingestion.domain.Boundary;
import dk.crowdcontrol.ingestion.domain.Event;
import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import dk.crowdcontrol.ingestion.domain.GeoPoint;
import dk.crowdcontrol.ingestion.domain.Position;
import dk.crowdcontrol.ingestion.domain.SessionId;
import dk.crowdcontrol.ingestion.domain.Zone;
import org.junit.jupiter.api.Test;

import java.time.Duration;
import java.time.Instant;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Tests {@link ReceivePositionService}: that valid positions are published with event and zones,
 * and that positions are rejected on rate limiting, unknown event or outside area and time span.
 */
class ReceivePositionServiceTest
{
  private static final Instant START = Instant.parse("2026-07-01T10:00:00Z");
  private static final Instant END = Instant.parse("2026-07-01T22:00:00Z");
  private static final Instant DURING = Instant.parse("2026-07-01T12:00:00Z");

  private final Zone zone = new Zone(UUID.randomUUID(), "Scene", square(0, 0, 1));
  private final Event event = new Event(UUID.randomUUID(), "Festival", START, END,
      Duration.ofSeconds(30), square(0, 0, 4), List.of(zone));

  private final FakeRateLimiter rateLimiter = new FakeRateLimiter();
  private final FakeActiveEventRepository repository = new FakeActiveEventRepository();
  private final FakePositionPublisher publisher = new FakePositionPublisher();
  private final ReceivePositionService service =
      new ReceivePositionService(rateLimiter, repository, publisher);

  @Test
  void validPositionIsPublishedWithEventAndZone()
  {
    repository.createOrReplace(event);
    Position position = positionAt(0.5, 0.5, DURING);

    service.receivePosition(position, event.getId());

    assertEquals(1, publisher.published.size());
    FilteredPosition published = publisher.published.getFirst();
    assertEquals(event.getId(), published.getEventId());
    assertEquals(List.of(zone.getId()), published.getZoneIds());
    assertEquals(position.getSessionId(), published.getPosition().getSessionId());
  }

  @Test
  void rateLimitedPositionIsRejectedBeforeEventLookup()
  {
    repository.createOrReplace(event);
    rateLimiter.allow = false;

    assertThrows(RateLimitExceededException.class,
        () -> service.receivePosition(positionAt(0.5, 0.5, DURING), event.getId()));

    assertEquals(0, repository.lookups);
    assertTrue(publisher.published.isEmpty());
  }

  @Test
  void unknownEventIsRejected()
  {
    assertThrows(OutsideActiveEventException.class,
        () -> service.receivePosition(positionAt(0.5, 0.5, DURING), UUID.randomUUID()));

    assertTrue(publisher.published.isEmpty());
  }

  @Test
  void positionOutsideEventAreaIsRejected()
  {
    repository.createOrReplace(event);

    assertThrows(OutsideActiveEventException.class,
        () -> service.receivePosition(positionAt(10, 10, DURING), event.getId()));

    assertTrue(publisher.published.isEmpty());
  }

  @Test
  void positionOutsideEventTimeIsRejected()
  {
    repository.createOrReplace(event);

    assertThrows(OutsideActiveEventException.class,
        () -> service.receivePosition(positionAt(0.5, 0.5, END.plusSeconds(1)), event.getId()));

    assertTrue(publisher.published.isEmpty());
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

  // Simple fakes of the out-ports, so the service can be tested without Spring, RabbitMQ or a database

  /** Allows or rejects all positions depending on the allow field. */
  private static class FakeRateLimiter implements RateLimiter
  {
    boolean allow = true;

    @Override public boolean allow(SessionId sessionId)
    {
      return allow;
    }
  }

  /** Events in a HashMap; counts lookups so the test can see whether a lookup happened. */
  private static class FakeActiveEventRepository implements ActiveEventRepository
  {
    private final Map<UUID, Event> events = new HashMap<>();
    int lookups = 0;

    @Override public Optional<Event> findById(UUID eventId)
    {
      lookups++;
      return Optional.ofNullable(events.get(eventId));
    }

    @Override public void createOrReplace(Event event)
    {
      events.put(event.getId(), event);
    }

    @Override public void delete(UUID eventId)
    {
      events.remove(eventId);
    }
  }

  /** Stores published positions in a list so the test can check them. */
  private static class FakePositionPublisher implements PositionPublisher
  {
    final List<FilteredPosition> published = new ArrayList<>();

    @Override public void publish(FilteredPosition filteredPosition)
    {
      published.add(filteredPosition);
    }
  }
}
