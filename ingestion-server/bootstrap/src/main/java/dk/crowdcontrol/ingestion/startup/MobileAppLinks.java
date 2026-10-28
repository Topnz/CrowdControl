package dk.crowdcontrol.ingestion.startup;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

/**
 * Builds and prints links to the mobile app. With the demo event turned on, the link gets
 * ?event=&lt;id&gt;, so it can be opened directly.
 */
@Component
public class MobileAppLinks
{
  private static final Logger log = LoggerFactory.getLogger(MobileAppLinks.class);

  private final boolean demoEventEnabled;
  private final String demoEventId;

  /**
   * @param demoEventEnabled whether the demo event is turned on (crowdcontrol.demo-event.enabled)
   * @param demoEventId      the demo event's ID (crowdcontrol.demo-event.id)
   */
  public MobileAppLinks(
      @Value("${crowdcontrol.demo-event.enabled:false}") boolean demoEventEnabled,
      @Value("${crowdcontrol.demo-event.id:}") String demoEventId)
  {
    this.demoEventEnabled = demoEventEnabled;
    this.demoEventId = demoEventId;
  }

  /**
   * Writes the link to the log surrounded by blank lines, so it is easy to find.
   *
   * @param label   description of the link, e.g. "Mobile app (this machine)"
   * @param baseUrl the server's address without a trailing slash, e.g. http://localhost:8080
   */
  public void print(String label, String baseUrl)
  {
    String link = demoEventEnabled ? baseUrl + "/?event=" + demoEventId : baseUrl + "/";
    log.info("\n\n    {}: {}\n", label, link);
  }
}
