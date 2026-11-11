package dk.crowdcontrol.ingestion.startup;

import dk.crowdcontrol.ingestion.application.port.in.SyncActiveEventsUseCase;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClientException;

/**
 * Fetches the active events from processing-server once at startup (GET /events/active), so
 * ingestion-server knows events that were created while it was not running (ID-13).
 * <p>
 * If processing-server cannot be reached, a warning is logged and ingestion-server starts without
 * its events (the demo event still works). This is deliberate, so ingestion-server does not depend
 * on the Api being started first; restart ingestion-server once the Api is running.
 */
@Component
class ActiveEventSync implements ApplicationRunner
{
  private static final Logger log = LoggerFactory.getLogger(ActiveEventSync.class);

  private final SyncActiveEventsUseCase syncActiveEventsUseCase;

  /** @param syncActiveEventsUseCase fetches the active events and stores them */
  ActiveEventSync(SyncActiveEventsUseCase syncActiveEventsUseCase)
  {
    this.syncActiveEventsUseCase = syncActiveEventsUseCase;
  }

  /**
   * Runs the sync and logs how many events were stored. Errors other than
   * {@link RestClientException} are not caught, since they are bugs and should stop startup.
   *
   * @param args the program arguments (not used)
   */
  @Override public void run(ApplicationArguments args)
  {
    try
    {
      int count = syncActiveEventsUseCase.sync();
      log.info("Synced {} active events from processing-server", count);
    }
    catch (RestClientException e)
    {
      // The cause's type (e.g. ConnectException) says more than its message, which can be null
      log.warn("Couldnt sync any active events from processing-server, runs without them: {} ({})",
          e.getMessage(), e.getMostSpecificCause().getClass().getSimpleName());
    }
  }
}
