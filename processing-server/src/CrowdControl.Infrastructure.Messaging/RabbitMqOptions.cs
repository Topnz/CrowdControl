namespace CrowdControl.Infrastructure.Messaging;

/// <summary>Sektionen "RabbitMq" i appsettings.json.</summary>
public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    /// <summary>Skal matche crowdcontrol.rabbitmq.positions-queue i ingestion-server.</summary>
    public string PositionsQueue { get; set; } = "positions";
}
