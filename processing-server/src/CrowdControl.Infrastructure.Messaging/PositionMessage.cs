using System.Text.Json;
using CrowdControl.Application;

namespace CrowdControl.Infrastructure.Messaging;

/// <summary>
/// Læser JSON-beskeden fra positions-køen (PosData i Java).
/// Web-defaults: camelCase og case-insensitive feltnavne, som Jackson skriver dem.
/// Alle felter er required i kontrakten, så manglende felter eller null giver JsonException
/// i stedet for stille standardværdier (fx latitude = 0).
/// </summary>
public static class PositionMessage
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true
    };

    /// <exception cref="JsonException">Hvis beskeden ikke er gyldig JSON eller mangler felter.</exception>
    public static PositionReceived Deserialize(ReadOnlySpan<byte> body)
    {
        return JsonSerializer.Deserialize<PositionReceived>(body, Json)
            ?? throw new JsonException("Beskeden er tom (null)");
    }
}
