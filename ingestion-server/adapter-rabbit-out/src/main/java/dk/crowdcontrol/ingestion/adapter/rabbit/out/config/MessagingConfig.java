package dk.crowdcontrol.ingestion.adapter.rabbit.out.config;

import org.springframework.amqp.core.Queue;
import org.springframework.amqp.support.converter.JacksonJsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.annotation.Profile;

/**
 * The queue for positions (Java -> C#), see contracts/asyncapi/messaging.yaml.
 * Messages are sent via RabbitMQ's default exchange, where routing key = queue name,
 * so no separate exchange or binding is needed.
 * RabbitTemplate is created by Spring Boot and automatically uses the converter bean below.
 */
@Configuration
@Profile("!offline")
public class MessagingConfig
{
  /**
   * Durable queue; created in RabbitMQ on the first connection if it does not exist.
   *
   * @param queueName the name of the positions queue (crowdcontrol.rabbitmq.positions-queue)
   * @return the queue
   */
  @Bean
  public Queue positionsQueue(@Value("${crowdcontrol.rabbitmq.positions-queue}") String queueName)
  {
    return new Queue(queueName);
  }

  /**
   * Sends messages as JSON (application/json) instead of serialised Java objects.
   *
   * @return the converter used by RabbitTemplate
   */
  @Bean
  public MessageConverter converter()
  {
    return new JacksonJsonMessageConverter();
  }
}
