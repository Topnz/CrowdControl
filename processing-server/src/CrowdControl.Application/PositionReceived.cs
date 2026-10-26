namespace CrowdControl.Application;

/// <summary>
/// En position, som ingestion-server har godkendt til et aktivt event.
/// Matcher contracts/schemas/rabbitMQ/PositionReceived.v1.json.
/// ZoneIds er tom, hvis positionen ligger i eventet, men uden for alle zoner.
/// </summary>
public record PositionReceived(
    Guid SessionId,
    Guid EventId,
    IReadOnlyList<Guid> ZoneIds,
    double Latitude,
    double Longitude,
    double Accuracy,
    DateTimeOffset Timestamp);
