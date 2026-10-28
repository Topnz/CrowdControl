package dk.crowdcontrol.ingestion.config;

import dk.crowdcontrol.ingestion.application.port.in.ReceivePositionUseCase;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.application.port.out.PositionPublisher;
import dk.crowdcontrol.ingestion.application.port.out.RateLimiter;
import dk.crowdcontrol.ingestion.application.service.ReceivePositionService;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Creates services from the application module as Spring beans. Services deliberately
 * have no Spring annotations, so they are wired here in the composition root.
 * The out-ports are provided by the adapters (Spring finds them by type).
 */
@Configuration
class UseCaseConfig
{
  /**
   * @param rateLimiter           InMemoryRateLimiter
   * @param activeEventRepository InMemoryActiveEventRepository
   * @param positionPublisher     RabbitPositionPublisher, or LoggingPositionPublisher with the "offline" profile
   * @return the use case used by PositionController
   */
  @Bean
  ReceivePositionUseCase receivePositionUseCase(RateLimiter rateLimiter,
      ActiveEventRepository activeEventRepository,
      PositionPublisher positionPublisher)
  {
    return new ReceivePositionService(rateLimiter, activeEventRepository, positionPublisher);
  }
}
