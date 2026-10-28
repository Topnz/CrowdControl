using System.Text.Json;
using CrowdControl.Api.Contracts;
using CrowdControl.Application.MonitorEvent;
using Json.Schema;

namespace CrowdControl.Api.IntegrationTests;

/// <summary>
/// Checks that the snapshot the Api sends to the dashboard matches
/// contracts/schemas/signalr/HeatmapSnapshot.v1.json. Fails if a property is renamed or
/// changes type in the DTOs without the contract being updated (or the other way round).
/// </summary>
public class HeatmapSnapshotContractTests
{
    private static readonly JsonSchema Schema =
        JsonSchema.FromText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "contracts", "HeatmapSnapshot.v1.json")));

    // Same serializer defaults as ASP.NET controllers and SignalR (camelCase)
    private static readonly JsonSerializerOptions WebJson = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    [Fact]
    public void SnapshotWithCellsAndZones_MatchesContract()
    {
        HeatmapSnapshot snapshot = new HeatmapSnapshot
        {
            EventName = "Test-event",
            Timestamp = DateTime.UtcNow,
            MinLat = 55.86,
            MinLon = 9.85,
            CellSizeLat = 0.0002,
            CellSizeLon = 0.00035,
            Cells = new List<GridCell> { new GridCell { X = 3, Y = 7, Count = 2 } },
            Zones = new List<ZoneCount>
            {
                new ZoneCount { Name = "With capacity", Capacity = 500, Count = 1 },
                new ZoneCount { Name = "Without capacity", Capacity = null, Count = 0 }
            }
        };

        AssertMatchesContract(snapshot.ToDto());
    }

    [Fact]
    public void EmptySnapshot_MatchesContract()
    {
        HeatmapSnapshot snapshot = new HeatmapSnapshot
        {
            EventName = "Test-event",
            Timestamp = DateTime.UtcNow,
            CellSizeLat = 0.0002,
            CellSizeLon = 0.00035
        };

        AssertMatchesContract(snapshot.ToDto());
    }

    private static void AssertMatchesContract(HeatmapSnapshotDto dto)
    {
        JsonElement json = JsonSerializer.SerializeToElement(dto, WebJson);

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
