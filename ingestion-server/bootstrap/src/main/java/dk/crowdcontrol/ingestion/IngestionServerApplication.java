package dk.crowdcontrol.ingestion;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * Starts ingestion-server. Lives at the top of dk.crowdcontrol.ingestion, so
 * component scanning finds the subpackages of all modules.
 */
@SpringBootApplication public class IngestionServerApplication
{
  /** @param args the program arguments, e.g. --spring.profiles.active=offline */
  public static void main(String[] args)
  {
    SpringApplication.run(IngestionServerApplication.class, args);
  }
}
