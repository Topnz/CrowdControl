package dk.crowdcontrol.ingestion.startup;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

/**
 * Bygger og printer links til mobil-appen. Med demo-eventet slået til får linket
 * ?event=<id>, så det kan åbnes direkte.
 */
@Component
public class MobileAppLinks
{
  private static final Logger log = LoggerFactory.getLogger(MobileAppLinks.class);

  private final boolean demoEventEnabled;
  private final String demoEventId;

  public MobileAppLinks(
      @Value("${crowdcontrol.demo-event.enabled:false}") boolean demoEventEnabled,
      @Value("${crowdcontrol.demo-event.id:}") String demoEventId)
  {
    this.demoEventEnabled = demoEventEnabled;
    this.demoEventId = demoEventId;
  }

  public void print(String label, String baseUrl)
  {
    String link = demoEventEnabled ? baseUrl + "/?event=" + demoEventId : baseUrl + "/";
    log.info("\n\n    {}: {}\n", label, link);
  }
}
