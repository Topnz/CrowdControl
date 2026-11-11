package dk.crowdcontrol.ingestion.adapter.processing;

import dk.crowdcontrol.ingestion.adapter.processing.dto.EventDto;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventSource;
import dk.crowdcontrol.ingestion.domain.Event;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Profile;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClient;

import java.util.List;
import java.util.Optional;

/**
 * Fetches the active events from processing-server with GET /events/active
 * (contracts/openapi/processing-api.yaml). Used in all profiles except "offline",
 * where EmptyActiveEventSource is used instead.
 * <p>
 * Connection and HTTP errors are passed on as RestClientException, so the caller decides
 * what happens when processing-server cannot be reached.
 */
@Component
@Profile("!offline")
public class ProcessingActiveEventSource implements ActiveEventSource
{
  private static final Logger log = LoggerFactory.getLogger(ProcessingActiveEventSource.class);

  private final RestClient restClient;

  /**
   * @param builder the RestClient builder from Spring Boot, preconfigured with Jackson
   * @param baseUrl processing-server's address (crowdcontrol.processing.base-url)
   */
  public ProcessingActiveEventSource(RestClient.Builder builder,
      @Value("${crowdcontrol.processing.base-url}") String baseUrl)
  {
    this.restClient = builder.baseUrl(baseUrl).build();
  }

  /**
   * Invalid events are logged and skipped, so one bad event does not stop the others.
   *
   * @return the valid active events; empty if processing-server sends none
   * @throws org.springframework.web.client.RestClientException if processing-server cannot be
   *                                                            reached or answers with an error
   */
  @Override public List<Event> fetchActiveEvents()
  {
    List<EventDto> dtos = restClient.get()
        .uri("/events/active")
        .retrieve()
        .body(new ParameterizedTypeReference<List<EventDto>>()
        {
        });

    if (dtos == null)
    {
      return List.of();
    }
    return dtos.stream().map(this::toDomainOrSkip)
        .flatMap(Optional::stream)
        .toList();
  }

  /** Maps one event, or logs a warning and returns empty if the domain rejects it. */
  private Optional<Event> toDomainOrSkip(EventDto dto)
  {
    try
    {
      return Optional.of(EventMapper.toDomain(dto));
    }
    catch (IllegalArgumentException | NullPointerException e)
    {
      log.warn("Springer event {} over, ugyldige data fra processing-server: {}",
          dto.eventId(), e.getMessage());
      return Optional.empty();
    }
  }
}
