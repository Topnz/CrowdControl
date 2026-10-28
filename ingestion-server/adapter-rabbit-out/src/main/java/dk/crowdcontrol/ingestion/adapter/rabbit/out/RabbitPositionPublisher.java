package dk.crowdcontrol.ingestion.adapter.rabbit.out;

import dk.crowdcontrol.ingestion.adapter.rabbit.out.dto.PosData;
import dk.crowdcontrol.ingestion.application.port.out.PositionPublisher;
import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import org.springframework.amqp.core.AmqpTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;

/**
 * Sender godkendte positioner som JSON til positions-køen, hvor processing-server henter dem.
 * Modstykket til LoggingPositionPublisher, som bruges med profilen "offline".
 */
@Component
@Profile("!offline")
public class RabbitPositionPublisher implements PositionPublisher
{
  private final AmqpTemplate amqpTemplate;
  private final String queueName;

  public RabbitPositionPublisher(AmqpTemplate amqpTemplate,
      @Value("${crowdcontrol.rabbitmq.positions-queue}") String queueName)
  {
    this.amqpTemplate = amqpTemplate;
    this.queueName = queueName;
  }

  @Override public void publish(FilteredPosition filteredPosition)
  {
    // Kun routing key = default exchange, som lægger beskeden i køen med samme navn
    amqpTemplate.convertAndSend(queueName, PosData.from(filteredPosition));
  }
}
