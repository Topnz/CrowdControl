package dk.crowdcontrol.ingestion.startup;

import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Component;

/**
 * Printer linket til mobil-appen på denne maskine, når serveren er klar.
 */
@Component
class LocalLinkPrinter
{
  private final MobileAppLinks links;

  LocalLinkPrinter(MobileAppLinks links)
  {
    this.links = links;
  }

  @EventListener(ApplicationReadyEvent.class)
  void printLink(ApplicationReadyEvent event)
  {
    String port = event.getApplicationContext().getEnvironment().getProperty("local.server.port");
    if (port == null) return; // ingen webserver, fx i @SpringBootTest
    links.print("Mobil-app (denne maskine)", "http://localhost:" + port);
  }
}
