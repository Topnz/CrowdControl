package dk.crowdcontrol.ingestion.application.service;

import dk.crowdcontrol.ingestion.application.port.in.OutsideActiveEventException;
import dk.crowdcontrol.ingestion.application.port.in.RateLimitExceededException;
import dk.crowdcontrol.ingestion.application.port.in.ReceivePositionUseCase;
import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.application.port.out.PositionPublisher;
import dk.crowdcontrol.ingestion.application.port.out.RateLimiter;
import dk.crowdcontrol.ingestion.domain.Event;
import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import dk.crowdcontrol.ingestion.domain.Position;

import java.util.UUID;

public class ReceivePositionService implements ReceivePositionUseCase
{
  private final RateLimiter rateLimiter;
  private final ActiveEventRepository activeEventRepository;
  private final PositionPublisher positionPublisher;

  public ReceivePositionService(RateLimiter rateLimiter,
      ActiveEventRepository activeEventRepository,
      PositionPublisher positionPublisher)
  {

    this.rateLimiter = rateLimiter;
    this.activeEventRepository = activeEventRepository;
    this.positionPublisher = positionPublisher;
  }

  @Override public void receivePosition(Position position, UUID eventId)
  {
    // 1. Rate limit først, så en enhed, der sender for ofte, ikke koster opslag
    if (!rateLimiter.allow(position.getSessionId()))
      throw new RateLimitExceededException();

    // 2. Find eventet
    Event event = activeEventRepository.findById(eventId)
        .orElseThrow(() -> new OutsideActiveEventException("Event " + eventId + " findes ikke"));

    // 3. Filtrér mod eventets område og tidsrum, og find zonerne (reglen ligger i domænet)
    FilteredPosition filteredPosition = FilteredPosition.from(event, position)
        .orElseThrow(() -> new OutsideActiveEventException(
            "Positionen ligger uden for område eller tidsrum for event " + eventId));

    // 4. Send videre til processing-server
    positionPublisher.publish(filteredPosition);
  }
}
