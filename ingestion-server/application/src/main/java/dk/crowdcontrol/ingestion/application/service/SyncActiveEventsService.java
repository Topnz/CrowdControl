package dk.crowdcontrol.ingestion.application.service;

import dk.crowdcontrol.ingestion.application.port.in.SyncActiveEventsUseCase;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventSource;
import dk.crowdcontrol.ingestion.domain.Event;

import java.util.List;

/**
 * Copies the active events from processing-server into ActiveEventRepository.
 * Deliberately has no Spring annotations; it is wired in UseCaseConfig in bootstrap.
 */
public class SyncActiveEventsService implements SyncActiveEventsUseCase
{
  private final ActiveEventSource activeEventSource;
  private final ActiveEventRepository activeEventRepository;

  /**
   * @param activeEventSource     where the active events are fetched from
   * @param activeEventRepository where the events are stored
   */
  public SyncActiveEventsService(ActiveEventSource activeEventSource,
      ActiveEventRepository activeEventRepository)
  {
    this.activeEventSource = activeEventSource;
    this.activeEventRepository = activeEventRepository;
  }

  @Override public int sync()
  {
    List<Event> activeEvents = activeEventSource.fetchActiveEvents();

    for (Event event : activeEvents)
    {
      activeEventRepository.createOrReplace(event);
    }
    return activeEvents.size();
  }
}
