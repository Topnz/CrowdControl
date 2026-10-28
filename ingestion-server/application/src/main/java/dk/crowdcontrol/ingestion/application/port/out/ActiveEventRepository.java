package dk.crowdcontrol.ingestion.application.port.out;

import dk.crowdcontrol.ingestion.domain.Event;

import java.util.Optional;
import java.util.UUID;

/**
 * Out-port: the active events known to ingestion-server, so positions can be checked
 * against the event's area and time span. To be populated from processing-server (REST at
 * startup and EventCreated/EventUpdated/EventDeleted on RabbitMQ).
 */
public interface ActiveEventRepository
{
  /**
   * @param eventId the event's ID
   * @return the event, or empty if it is not known
   */
  Optional<Event> findById(UUID eventId);

  /**
   * Stores the event, or replaces it if one with the same ID already exists
   * (EventCreated/EventUpdated).
   *
   * @param event the event to store
   */
  void createOrReplace(Event event);

  /**
   * Removes the event (EventDeleted). Does nothing if it does not exist.
   *
   * @param eventId the ID of the event to remove
   */
  void delete(UUID eventId);
}
