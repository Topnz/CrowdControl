package dk.crowdcontrol.ingestion.startup;

import jakarta.annotation.PreDestroy;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Component;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Starter en Cloudflare quick tunnel (cloudflared) til serveren og printer det offentlige
 * HTTPS-link til mobil-appen. Tunnelen lukkes sammen med serveren.
 * Slås til med crowdcontrol.tunnel.enabled (profilen "lan").
 */
@Component
@ConditionalOnProperty(name = "crowdcontrol.tunnel.enabled", havingValue = "true")
class CloudflareTunnel
{
  private static final Logger log = LoggerFactory.getLogger(CloudflareTunnel.class);

  // cloudflared skriver det tilfældige link i sin log, fx https://abc-def.trycloudflare.com
  private static final Pattern TUNNEL_URL = Pattern.compile("https://[a-z0-9-]+\\.trycloudflare\\.com");

  private final MobileAppLinks links;
  private Process process;

  CloudflareTunnel(MobileAppLinks links)
  {
    this.links = links;
  }

  @EventListener(ApplicationReadyEvent.class)
  void start(ApplicationReadyEvent event)
  {
    String port = event.getApplicationContext().getEnvironment().getProperty("local.server.port");
    try
    {
      process = new ProcessBuilder("cloudflared", "tunnel", "--no-autoupdate", "--url", "http://localhost:" + port)
          .redirectErrorStream(true)
          .start();
    }
    catch (IOException e)
    {
      log.warn("""
          cloudflared blev ikke fundet, så der er ingen tunnel. Installér den og genstart:
            Windows: winget install --id Cloudflare.cloudflared
            macOS:   brew install cloudflared
            Linux:   https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/""");
      return;
    }

    log.info("Starter Cloudflare-tunnel ...");
    Thread reader = new Thread(this::readOutput, "cloudflared");
    reader.setDaemon(true);
    reader.start();
  }

  // Læser hele outputtet (også efter linket), ellers blokerer cloudflared, når bufferen er fuld
  private void readOutput()
  {
    boolean found = false;
    try (BufferedReader output = new BufferedReader(new InputStreamReader(process.getInputStream())))
    {
      String line;
      while ((line = output.readLine()) != null)
      {
        log.debug(line);
        Matcher matcher = TUNNEL_URL.matcher(line);
        if (!found && matcher.find())
        {
          found = true;
          links.print("Mobil-app (offentligt link via Cloudflare-tunnel)", matcher.group());
        }
      }
    }
    catch (IOException e)
    {
      // Strømmen lukkes, når processen stoppes
    }
    if (!found)
    {
      log.warn("Cloudflare-tunnelen stoppede uden at give et link. Kør med logging.level.dk.crowdcontrol=DEBUG for at se cloudflareds output.");
    }
  }

  @PreDestroy
  void stop()
  {
    if (process != null)
    {
      process.destroy();
    }
  }
}
