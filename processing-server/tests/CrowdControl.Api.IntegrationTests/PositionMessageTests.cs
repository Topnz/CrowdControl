using System.Text;
using System.Text.Json;
using CrowdControl.Application;
using CrowdControl.Infrastructure.Messaging;

namespace CrowdControl.Api.IntegrationTests;

/// <summary>
/// Checks that C# can read the JSON that ingestion-server sends (PositionReceived.v1.json).
/// The JSON is the same as in PosDataTest in adapter-rabbit-out, so both sides are tested against the same message.
/// </summary>
public class PositionMessageTests
{
    private const string JsonFromJava =
        "{\"sessionId\":\"3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b\","
        + "\"eventId\":\"00000000-0000-0000-0000-000000000001\","
        + "\"zoneIds\":[\"00000000-0000-0000-0000-00000000000a\"],"
        + "\"latitude\":55.86,\"longitude\":9.85,\"accuracy\":8.5,"
        + "\"timestamp\":\"2026-10-24T18:22:18Z\"}";

    [Fact]
    public void ReadsMessageFromJava()
    {
        PositionReceived position = PositionMessage.Deserialize(Encoding.UTF8.GetBytes(JsonFromJava));

        Assert.Equal(Guid.Parse("3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b"), position.SessionId);
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), position.EventId);
        Assert.Equal([Guid.Parse("00000000-0000-0000-0000-00000000000a")], position.ZoneIds);
        Assert.Equal(55.86, position.Latitude);
        Assert.Equal(9.85, position.Longitude);
        Assert.Equal(8.5, position.Accuracy);
        Assert.Equal(new DateTimeOffset(2026, 10, 24, 18, 22, 18, TimeSpan.Zero), position.Timestamp);
    }

    [Fact]
    public void ReadsEmptyZoneIds()
    {
        string json = JsonFromJava.Replace("[\"00000000-0000-0000-0000-00000000000a\"]", "[]");

        PositionReceived position = PositionMessage.Deserialize(Encoding.UTF8.GetBytes(json));

        Assert.Empty(position.ZoneIds);
    }

    [Fact]
    public void RejectsMissingField()
    {
        string json = JsonFromJava.Replace("\"latitude\":55.86,", "");

        Assert.Throws<JsonException>(() => PositionMessage.Deserialize(Encoding.UTF8.GetBytes(json)));
    }

    [Fact]
    public void RejectsNullZoneIds()
    {
        string json = JsonFromJava.Replace("[\"00000000-0000-0000-0000-00000000000a\"]", "null");

        Assert.Throws<JsonException>(() => PositionMessage.Deserialize(Encoding.UTF8.GetBytes(json)));
    }
}
