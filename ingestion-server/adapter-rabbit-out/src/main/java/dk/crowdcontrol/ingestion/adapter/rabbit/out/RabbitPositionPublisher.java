package dk.crowdcontrol.ingestion.adapter.rabbit.out;

import dk.crowdcontrol.ingestion.adapter.rabbit.out.dto.PosData;
import dk.crowdcontrol.ingestion.application.port.out.PositionPublisher;
import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import org.springframework.amqp.core.AmqpTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;

/**
 * Sends accepted positions as JSON to the positions queue, where processing-server picks them up.
 * The counterpart of LoggingPositionPublisher, which is used with the "offline" profile.
 */
@Component
@Profile("!offline")
public class RabbitPositionPublisher implements PositionPublisher
{
  private final AmqpTemplate amqpTemplate;
  private final String queueName;

  /**
   * @param amqpTemplate sends messages to RabbitMQ; created by Spring Boot
   * @param queueName    the name of the positions queue (crowdcontrol.rabbitmq.positions-queue)
   */
  public RabbitPositionPublisher(AmqpTemplate amqpTemplate,
      @Value("${crowdcontrol.rabbitmq.positions-queue}") String queueName)
  {
    this.amqpTemplate = amqpTemplate;
    this.queueName = queueName;
  }

  @Override public void publish(FilteredPosition filteredPosition)
  {
    // Only a routing key = default exchange, which puts the message in the queue with the same name
    amqpTemplate.convertAndSend(queueName, PosData.from(filteredPosition));
  }
}
