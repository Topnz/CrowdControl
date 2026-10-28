using System.Text.Json;
using CrowdControl.Dashboard.Contracts;
using Json.Schema;

namespace CrowdControl.Dashboard.Tests;

/// <summary>
/// Checks the dashboard's copy of the snapshot DTOs against
/// contracts/schemas/signalr/HeatmapSnapshot.v1.json. processing-server has the same test for
/// its own DTOs, so both sides are tested against the same contract.
/// </summary>
public class HeatmapSnapshotContractTests
{
    private static readonly JsonSchema Schema =
        JsonSchema.FromText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "contracts", "HeatmapSnapshot.v1.json")));

    // camelCase like the JSON the Api sends via SignalR and REST
    private static readonly JsonSerializerOptions WebJson = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private const string SnapshotFromApi =
        "{\"eventName\":\"Test-event\",\"timestamp\":\"2026-10-28T12:20:23.9532458Z\","
        + "\"minLat\":55.86,\"minLon\":9.85,\"cellSizeLat\":0.0002,\"cellSizeLon\":0.00035,"
        + "\"cells\":[{\"x\":3,\"y\":7,\"count\":2}],"
        + "\"zones\":[{\"name\":\"With capacity\",\"capacity\":500,\"count\":1},"
        + "{\"name\":\"Without capacity\",\"capacity\":null,\"count\":0}]}";

    [Fact]
    public void ReadsSnapshotThatMatchesContract()
    {
        AssertMatchesContract(JsonDocument.Parse(SnapshotFromApi).RootElement);

        HeatmapSnapshotDto snapshot = JsonSerializer.Deserialize<HeatmapSnapshotDto>(SnapshotFromApi, WebJson)!;

        Assert.Equal("Test-event", snapshot.EventName);
        Assert.Equal(55.86, snapshot.MinLat);
        Assert.Equal(0.00035, snapshot.CellSizeLon);
        CellDto cell = Assert.Single(snapshot.Cells);
        Assert.Equal((3, 7, 2), (cell.X, cell.Y, cell.Count));
        Assert.Equal(500, snapshot.Zones[0].Capacity);
        Assert.Null(snapshot.Zones[1].Capacity);
    }

    // Deserializing alone would silently ignore a renamed property, so the DTOs are also
    // serialized and validated: a property that is not in the contract makes this fail.
    [Fact]
    public void DtoPropertiesMatchContract()
    {
        HeatmapSnapshotDto snapshot = JsonSerializer.Deserialize<HeatmapSnapshotDto>(SnapshotFromApi, WebJson)!;

        AssertMatchesContract(JsonSerializer.SerializeToElement(snapshot, WebJson));
    }

    private static void AssertMatchesContract(JsonElement json)
    {
        EvaluationResults result = Schema.Evaluate(json, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true
        });

        IEnumerable<string> errors = (result.Details ?? [])
            .Where(detail => detail.Errors != null)
            .SelectMany(detail => detail.Errors!.Select(error => $"{detail.InstanceLocation}: {error.Value}"));
        Assert.True(result.IsValid, $"Snapshot does not match HeatmapSnapshot.v1.json:\n{string.Join("\n", errors)}\n{json}");
    }
}
