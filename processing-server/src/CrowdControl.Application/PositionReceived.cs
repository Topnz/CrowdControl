namespace CrowdControl.Application;

/// <summary>
/// A position that ingestion-server has accepted for an active event.
/// Matches contracts/schemas/rabbitMQ/PositionReceived.v1.json.
/// ZoneIds is empty if the position is inside the event but outside all zones.
/// </summary>
public record PositionReceived(
    Guid SessionId,
    Guid EventId,
    IReadOnlyList<Guid> ZoneIds,
    double Latitude,
    double Longitude,
    double Accuracy,
    DateTimeOffset Timestamp);
