package dk.crowdcontrol.ingestion.application.port.out;

import dk.crowdcontrol.ingestion.domain.Event;

import java.util.Optional;
import java.util.UUID;

public interface ActiveEventRepository
{
  Optional<Event> findById(UUID eventId);
  void createOrReplace(Event event);
  void delete(UUID eventId);
}
