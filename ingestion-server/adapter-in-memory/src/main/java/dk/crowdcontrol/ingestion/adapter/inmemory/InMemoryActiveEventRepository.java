package dk.crowdcontrol.ingestion.adapter.inmemory;

import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.domain.Event;
import org.springframework.stereotype.Component;

import java.util.Map;
import java.util.Optional;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Keeps active events in memory. Used in all profiles until
 * adapter-persistence-postgis has an implementation; then this one should get @Profile("offline").
 * ConcurrentHashMap, because requests are handled on several threads at once.
 */
@Component
public class InMemoryActiveEventRepository implements ActiveEventRepository
{
  private final Map<UUID, Event> activeEvents = new ConcurrentHashMap<>();

  @Override public Optional<Event> findById(UUID eventId)
  {
    return Optional.ofNullable(activeEvents.get(eventId));
  }

  @Override public void createOrReplace(Event event)
  {
    activeEvents.put(event.getId(), event);
  }

  @Override public void delete(UUID eventId)
  {
    activeEvents.remove(eventId);
  }
}
