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
/// Listens to the positions queue (Java -> C#, see contracts/asyncapi/messaging.yaml) and
/// sends each position to IReceivePositionUseCase. The message is only acked once it has been
/// processed, so a position is not lost if the server stops halfway.
/// </summary>
public class RabbitMqPositionConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqPositionConsumer> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    // How many messages RabbitMQ may send before they are acked
    private const ushort PrefetchCount = 50;

    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using IConnection connection = await ConnectAsync(stoppingToken);
        await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Same arguments as Java's new Queue("positions"), otherwise RabbitMQ rejects with PRECONDITION_FAILED
        await channel.QueueDeclareAsync(_options.PositionsQueue,
            durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, PrefetchCount, global: false, stoppingToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken);

        await channel.BasicConsumeAsync(_options.PositionsQueue, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Lytter på køen {Queue} på {Host}:{Port}", _options.PositionsQueue, _options.Host, _options.Port);

        // The consumer runs on the RabbitMQ client's threads; here we just wait until the server shuts down
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
            // The server is shutting down: the message is not acked, so RabbitMQ delivers it again later
        }
        catch (JsonException e)
        {
            // The message does not follow the contract and will never become valid, so it is discarded
            logger.LogWarning(e, "Ugyldig besked på {Queue} kasseres", _options.PositionsQueue);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
        }
        catch (Exception e)
        {
            // No requeue: a position is only interesting right now, and requeue can cause an infinite loop
            logger.LogError(e, "Position fra {Queue} kunne ikke behandles og kasseres", _options.PositionsQueue);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
        }
    }

    /// <summary>
    /// Retries until RabbitMQ responds, so the server can be started before docker compose.
    /// If the connection is lost later, the client restores it by itself (AutomaticRecoveryEnabled).
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
