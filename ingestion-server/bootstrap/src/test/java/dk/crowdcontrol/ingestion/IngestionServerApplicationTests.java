package dk.crowdcontrol.ingestion;

import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;

/**
 * Checks that the Spring context can start with the "offline" profile, i.e. without RabbitMQ,
 * a database and processing-server, and that all beans can be wired.
 */
@SpringBootTest @ActiveProfiles("offline") class IngestionServerApplicationTests
{

  @Test void contextLoads()
  {
  }

}
