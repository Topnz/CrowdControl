namespace CrowdControl.Infrastructure.Messaging;

/// <summary>The "RabbitMq" section in appsettings.json.</summary>
public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    /// <summary>Must match crowdcontrol.rabbitmq.positions-queue in ingestion-server.</summary>
    public string PositionsQueue { get; set; } = "positions";
}
