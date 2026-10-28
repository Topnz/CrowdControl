package dk.crowdcontrol.ingestion.adapter.inmemory;

import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.domain.Event;
import org.springframework.stereotype.Component;

import java.util.Map;
import java.util.Optional;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Holder aktive events i hukommelsen. Bruges i alle profiler, indtil
 * adapter-persistence-postgis har en implementering; så skal denne have @Profile("offline").
 * ConcurrentHashMap, fordi requests håndteres i flere tråde samtidig.
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
