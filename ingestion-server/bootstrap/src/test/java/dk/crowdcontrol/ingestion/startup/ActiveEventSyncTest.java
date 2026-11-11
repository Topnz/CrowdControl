package dk.crowdcontrol.ingestion.startup;

import dk.crowdcontrol.ingestion.application.port.in.SyncActiveEventsUseCase;
import org.junit.jupiter.api.Test;
import org.springframework.web.client.ResourceAccessException;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Tests {@link ActiveEventSync}: that the sync runs at startup, that startup continues when
 * processing-server cannot be reached, and that other errors are not hidden.
 */
class ActiveEventSyncTest
{
  @Test
  void syncRunsOnce()
  {
    FakeSyncActiveEventsUseCase useCase = new FakeSyncActiveEventsUseCase(null);

    new ActiveEventSync(useCase).run(null);

    assertEquals(1, useCase.calls);
  }

  @Test
  void unreachableProcessingServerDoesNotStopStartup()
  {
    FakeSyncActiveEventsUseCase useCase =
        new FakeSyncActiveEventsUseCase(new ResourceAccessException("Connection refused"));

    assertDoesNotThrow(() -> new ActiveEventSync(useCase).run(null));
    assertEquals(1, useCase.calls);
  }

  @Test
  void otherErrorsAreNotCaught()
  {
    FakeSyncActiveEventsUseCase useCase =
        new FakeSyncActiveEventsUseCase(new IllegalStateException("bug"));

    assertThrows(IllegalStateException.class, () -> new ActiveEventSync(useCase).run(null));
  }

  /** Counts calls and throws the given exception, if any, instead of syncing. */
  private static class FakeSyncActiveEventsUseCase implements SyncActiveEventsUseCase
  {
    private final RuntimeException toThrow;
    int calls = 0;

    FakeSyncActiveEventsUseCase(RuntimeException toThrow)
    {
      this.toThrow = toThrow;
    }

    @Override public int sync()
    {
      calls++;
      if (toThrow != null) throw toThrow;
      return 0;
    }
  }
}
