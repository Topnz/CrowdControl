using System.Text.Json;
using CrowdControl.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace CrowdControl.Infrastructure.Messaging;

/// <summary>
/// Lytter på positions-køen (Java -> C#, se contracts/asyncapi/messaging.yaml) og
/// sender hver position til IReceivePositionUseCase. Beskeden ack'es først, når den er
/// behandlet, så en position ikke går tabt, hvis serveren stopper midt i.
/// </summary>
public class RabbitMqPositionConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqPositionConsumer> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    // Hvor mange beskeder RabbitMQ må sende, før de er ack'et
    private const ushort PrefetchCount = 50;

    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using IConnection connection = await ConnectAsync(stoppingToken);
        await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Samme argumenter som Javas new Queue("positions"), ellers afviser RabbitMQ med PRECONDITION_FAILED
        await channel.QueueDeclareAsync(_options.PositionsQueue,
            durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, PrefetchCount, global: false, stoppingToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken);

        await channel.BasicConsumeAsync(_options.PositionsQueue, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Lytter på køen {Queue} på {Host}:{Port}", _options.PositionsQueue, _options.Host, _options.Port);

        // Consumeren kører i RabbitMQ-klientens tråde; her ventes bare til serveren lukker
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        try
        {
            PositionReceived position = PositionMessage.Deserialize(delivery.Body.Span);

            using IServiceScope scope = scopeFactory.CreateScope();
            IReceivePositionUseCase useCase = scope.ServiceProvider.GetRequiredService<IReceivePositionUseCase>();
            await useCase.HandleAsync(position, stoppingToken);

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Serveren lukker: beskeden er ikke ack'et, så RabbitMQ leverer den igen senere
        }
        catch (JsonException e)
        {
            // Beskeden følger ikke kontrakten og bliver aldrig gyldig, så den kasseres
            logger.LogWarning(e, "Ugyldig besked på {Queue} kasseres", _options.PositionsQueue);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
        }
        catch (Exception e)
        {
            // Ikke requeue: en position er kun interessant lige nu, og requeue kan give en uendelig løkke
            logger.LogError(e, "Position fra {Queue} kunne ikke behandles og kasseres", _options.PositionsQueue);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
        }
    }

    /// <summary>
    /// Prøver igen, indtil RabbitMQ svarer, så serveren kan startes før docker compose.
    /// Mister vi forbindelsen senere, genopretter klienten den selv (AutomaticRecoveryEnabled).
    /// </summary>
    private async Task<IConnection> ConnectAsync(CancellationToken stoppingToken)
    {
        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            ClientProvidedName = "processing-server"
        };

        while (true)
        {
            try
            {
                return await factory.CreateConnectionAsync(stoppingToken);
            }
            catch (BrokerUnreachableException e)
            {
                logger.LogWarning("RabbitMQ på {Host}:{Port} svarer ikke ({Message}). Prøver igen om {Delay}s",
                    _options.Host, _options.Port, e.Message, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }
}
