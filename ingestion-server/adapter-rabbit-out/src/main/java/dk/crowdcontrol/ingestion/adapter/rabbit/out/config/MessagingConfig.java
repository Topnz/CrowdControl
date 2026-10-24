package dk.crowdcontrol.ingestion.adapter.rabbit.out.config;

import org.springframework.amqp.core.Queue;
import org.springframework.amqp.support.converter.JacksonJsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.annotation.Profile;

/**
 * Køen til positioner (Java -> C#), se contracts/asyncapi/messaging.yaml.
 * Der sendes via RabbitMQ's default exchange, hvor routing key = kønavn,
 * så der er ikke brug for en egen exchange eller binding.
 * RabbitTemplate laves af Spring Boot og bruger automatisk converter-beanen nedenfor.
 */
@Configuration
@Profile("!local")
public class MessagingConfig
{
  /** Durable kø; oprettes i RabbitMQ ved første forbindelse, hvis den ikke findes. */
  @Bean
  public Queue positionsQueue(@Value("${crowdcontrol.rabbitmq.positions-queue}") String queueName)
  {
    return new Queue(queueName);
  }

  /** Sender beskeder som JSON (application/json) i stedet for serialiserede Java-objekter. */
  @Bean
  public MessageConverter converter()
  {
    return new JacksonJsonMessageConverter();
  }
}
