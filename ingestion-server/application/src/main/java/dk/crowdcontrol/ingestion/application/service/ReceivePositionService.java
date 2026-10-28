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

/**
 * Accepts positions: rate limiting, event lookup, filtering against area
 * and time span, and finally publishing to processing-server.
 * Deliberately has no Spring annotations; it is wired in UseCaseConfig in bootstrap.
 */
public class ReceivePositionService implements ReceivePositionUseCase
{
  private final RateLimiter rateLimiter;
  private final ActiveEventRepository activeEventRepository;
  private final PositionPublisher positionPublisher;

  /**
   * @param rateLimiter           limits how often one device may send
   * @param activeEventRepository the active events positions are checked against
   * @param positionPublisher     forwards accepted positions
   */
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
    // 1. Rate limit first, so a device that sends too often does not cost a lookup
    if (!rateLimiter.allow(position.getSessionId()))
      throw new RateLimitExceededException();

    // 2. Find the event
    Event event = activeEventRepository.findById(eventId)
        .orElseThrow(() -> new OutsideActiveEventException("Event " + eventId + " findes ikke"));

    // 3. Filter against the event's area and time span, and find the zones (the rule lives in the domain)
    FilteredPosition filteredPosition = FilteredPosition.from(event, position)
        .orElseThrow(() -> new OutsideActiveEventException(
            "Positionen ligger uden for område eller tidsrum for event " + eventId));

    // 4. Forward to processing-server
    positionPublisher.publish(filteredPosition);
  }
}
