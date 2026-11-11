package dk.crowdcontrol.ingestion.application.service;

import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventSource;
import dk.crowdcontrol.ingestion.domain.Boundary;
import dk.crowdcontrol.ingestion.domain.Event;
import dk.crowdcontrol.ingestion.domain.GeoPoint;
import org.junit.jupiter.api.Test;

import java.time.Duration;
import java.time.Instant;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Tests {@link SyncActiveEventsService}: that the events fetched from processing-server are
 * stored in the repository, that existing events are replaced, and that the count is returned.
 */
class SyncActiveEventsServiceTest
{
  private static final Instant START = Instant.parse("2026-07-01T10:00:00Z");
  private static final Instant END = Instant.parse("2026-07-01T22:00:00Z");

  private final FakeActiveEventSource source = new FakeActiveEventSource();
  private final FakeActiveEventRepository repository = new FakeActiveEventRepository();
  private final SyncActiveEventsService service = new SyncActiveEventsService(source, repository);

  @Test
  void allFetchedEventsAreStored()
  {
    Event first = event(UUID.randomUUID(), "Roskilde");
    Event second = event(UUID.randomUUID(), "Smukfest");
    source.events = List.of(first, second);

    int synced = service.sync();

    assertEquals(2, synced);
    assertEquals(Optional.of(first), repository.findById(first.getId()));
    assertEquals(Optional.of(second), repository.findById(second.getId()));
  }

  @Test
  void noEventsReturnsZero()
  {
    source.events = List.of();

    int synced = service.sync();

    assertEquals(0, synced);
    assertTrue(repository.events.isEmpty());
  }

  @Test
  void existingEventIsReplaced()
  {
    UUID id = UUID.randomUUID();
    repository.createOrReplace(event(id, "Old name"));
    source.events = List.of(event(id, "New name"));

    int synced = service.sync();

    assertEquals(1, synced);
    assertEquals(1, repository.events.size());
    assertEquals("New name", repository.findById(id).orElseThrow().getName());
  }

  /** Valid event with the given ID and name, a 1 x 1 degree area and no zones. */
  private static Event event(UUID id, String name)
  {
    return new Event(id, name, START, END, Duration.ofSeconds(30), square(0, 0, 1), List.of());
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

  // Simple fakes of the out-ports, so the service can be tested without Spring or HTTP

  /** Returns the events in the list, so the test decides what processing-server "sends". */
  private static class FakeActiveEventSource implements ActiveEventSource
  {
    List<Event> events = List.of();

    @Override public List<Event> fetchActiveEvents()
    {
      return events;
    }
  }

  /** Events in a HashMap, which the test can inspect directly. */
  private static class FakeActiveEventRepository implements ActiveEventRepository
  {
    final Map<UUID, Event> events = new HashMap<>();

    @Override public Optional<Event> findById(UUID eventId)
    {
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
}
