package dk.crowdcontrol.ingestion.application.port.in;

/**
 * In-port: fetch all active events from processing-server and store them, so positions can be
 * checked against them (ID-13). Called by ActiveEventSync at startup and implemented by
 * SyncActiveEventsService.
 */
public interface SyncActiveEventsUseCase
{
  /**
   * Fetches the active events and stores each one, replacing an event with the same ID.
   *
   * @return the number of active events received from processing-server and stored in this call
   */
  int sync();
}
