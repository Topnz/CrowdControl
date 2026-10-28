package dk.crowdcontrol.ingestion.startup;

import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Component;

/**
 * Prints the link to the mobile app on this machine when the server is ready.
 */
@Component
class LocalLinkPrinter
{
  private final MobileAppLinks links;

  /** @param links builds and prints the link */
  LocalLinkPrinter(MobileAppLinks links)
  {
    this.links = links;
  }

  /**
   * Prints http://localhost:&lt;port&gt;. Prints nothing if no web server is running.
   *
   * @param event gives access to the port the server is actually listening on
   */
  @EventListener(ApplicationReadyEvent.class)
  void printLink(ApplicationReadyEvent event)
  {
    String port = event.getApplicationContext().getEnvironment().getProperty("local.server.port");
    if (port == null) return; // no web server, e.g. in @SpringBootTest
    links.print("Mobil-app (denne maskine)", "http://localhost:" + port);
  }
}
