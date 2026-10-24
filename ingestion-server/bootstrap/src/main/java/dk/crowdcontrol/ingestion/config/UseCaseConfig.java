package dk.crowdcontrol.ingestion.config;

import dk.crowdcontrol.ingestion.application.port.in.ReceivePositionUseCase;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.application.port.out.PositionPublisher;
import dk.crowdcontrol.ingestion.application.port.out.RateLimiter;
import dk.crowdcontrol.ingestion.application.service.ReceivePositionService;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Laver services fra application-modulet som Spring-beans. Services har bevidst
 * ingen Spring-annotationer, så de wires her i composition root.
 * Out-portene leveres af adapterne (Spring finder dem ud fra typen).
 */
@Configuration
class UseCaseConfig
{
  @Bean
  ReceivePositionUseCase receivePositionUseCase(RateLimiter rateLimiter,
      ActiveEventRepository activeEventRepository,
      PositionPublisher positionPublisher)
  {
    return new ReceivePositionService(rateLimiter, activeEventRepository, positionPublisher);
  }
}
