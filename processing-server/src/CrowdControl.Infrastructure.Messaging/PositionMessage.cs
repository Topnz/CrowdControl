using System.Text.Json;
using CrowdControl.Application;

namespace CrowdControl.Infrastructure.Messaging;

/// <summary>
/// Reads the JSON message from the positions queue (PosData in Java).
/// Web defaults: camelCase and case-insensitive field names, as Jackson writes them.
/// All fields are required in the contract, so missing fields or null give a JsonException
/// instead of silent default values (e.g. latitude = 0).
/// </summary>
public static class PositionMessage
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true
    };

    /// <exception cref="JsonException">If the message is not valid JSON or is missing fields.</exception>
    public static PositionReceived Deserialize(ReadOnlySpan<byte> body)
    {
        return JsonSerializer.Deserialize<PositionReceived>(body, Json)
            ?? throw new JsonException("Beskeden er tom (null)");
    }
}
