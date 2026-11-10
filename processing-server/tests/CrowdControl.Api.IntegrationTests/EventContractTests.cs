using System.Text.Json;
using CrowdControl.Api.Contracts;
using Json.Schema;

namespace CrowdControl.Api.IntegrationTests;

/// <summary>
/// Checks the DTOs of POST /events (ID-13) against contracts/: the request body against
/// schemas/rest/CreateEventRequest.v1.json and the 201 response against Event in
/// schemas/shared/common.v1.json. Fails if a property is renamed or changes type in the DTOs
/// without the contract being updated (or the other way round).
/// </summary>
public class EventContractTests
{
    private static readonly string ContractsFolder = Path.Combine(AppContext.BaseDirectory, "contracts");

    // Same serializer defaults as ASP.NET controllers (camelCase)
    private static readonly JsonSerializerOptions WebJson = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private static readonly JsonSchema RequestSchema;
    private static readonly JsonSchema EventSchema;

    static EventContractTests()
    {
        // CreateEventRequest refers to ../shared/common.v1.json, which is resolved through the
        // registry by its $id, so common.v1.json is registered first.
        JsonSchema common = JsonSchema.FromText(File.ReadAllText(Path.Combine(ContractsFolder, "common.v1.json")));
        SchemaRegistry.Global.Register(common);

        RequestSchema = JsonSchema.FromText(File.ReadAllText(Path.Combine(ContractsFolder, "CreateEventRequest.v1.json")));
        EventSchema = JsonSchema.FromText(
            """{ "$ref": "https://crowdcontrol.dk/schemas/shared/common.v1.json#/$defs/Event" }""");
    }

    [Fact]
    public void CreateEventRequest_MatchesContract()
    {
        AssertValid(RequestSchema, ValidRequest(), "CreateEventRequest.v1.json");
    }

    // The dashboard may send Danish time with an offset instead of "Z"; both are valid date-time
    [Fact]
    public void CreateEventRequestWithOffset_MatchesContractAndConvertsToUtc()
    {
        CreateEventRequestDto request = ValidRequest();
        request.StartTime = new DateTimeOffset(2026, 7, 1, 12, 0, 0, TimeSpan.FromHours(2));

        AssertValid(RequestSchema, request, "CreateEventRequest.v1.json");
        Assert.Equal(new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc), request.StartTime.UtcDateTime);
        Assert.Equal(DateTimeKind.Utc, request.StartTime.UtcDateTime.Kind);
    }

    // Guards against a schema that accepts anything, e.g. a $ref that resolves to nothing
    [Fact]
    public void CreateEventRequestWithTwoPoints_IsRejectedByContract()
    {
        CreateEventRequestDto request = ValidRequest();
        request.Area.RemoveAt(2);

        Assert.False(Evaluate(RequestSchema, request).IsValid, "The contract should require at least 3 points in area");
    }

    [Fact]
    public void CreateEventRequestWithEmptyName_IsRejectedByContract()
    {
        CreateEventRequestDto request = ValidRequest();
        request.Name = "";

        Assert.False(Evaluate(RequestSchema, request).IsValid, "The contract should reject an empty name");
    }

    [Fact]
    public void EventWithoutZones_MatchesContract()
    {
        CreateEventRequestDto request = ValidRequest();
        EventDto response = new EventDto
        {
            EventId = Guid.NewGuid(),
            Name = request.Name,
            StartTime = request.StartTime.UtcDateTime,
            EndTime = request.EndTime.UtcDateTime,
            SendIntervalSeconds = request.SendIntervalSeconds,
            Area = request.Area,
            Zones = []
        };

        AssertValid(EventSchema, response, "Event in common.v1.json");
    }

    [Fact]
    public void EventWithZone_MatchesContract()
    {
        CreateEventRequestDto request = ValidRequest();
        EventDto response = new EventDto
        {
            EventId = Guid.NewGuid(),
            Name = request.Name,
            StartTime = request.StartTime.UtcDateTime,
            EndTime = request.EndTime.UtcDateTime,
            SendIntervalSeconds = request.SendIntervalSeconds,
            Area = request.Area,
            Zones = [new ZoneDto { ZoneId = Guid.NewGuid(), Name = "Stage A", Area = request.Area }]
        };

        AssertValid(EventSchema, response, "Event in common.v1.json");
    }

    private static CreateEventRequestDto ValidRequest()
    {
        return new CreateEventRequestDto
        {
            Name = "Test-event",
            StartTime = new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 7, 1, 22, 0, 0, TimeSpan.Zero),
            SendIntervalSeconds = 10,
            Area =
            [
                new GeoPointDto { Latitude = 55.860, Longitude = 9.850 },
                new GeoPointDto { Latitude = 55.865, Longitude = 9.850 },
                new GeoPointDto { Latitude = 55.865, Longitude = 9.858 }
            ]
        };
    }

    private static EvaluationResults Evaluate<T>(JsonSchema schema, T dto)
    {
        JsonElement json = JsonSerializer.SerializeToElement(dto, WebJson);

        return schema.Evaluate(json, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true
        });
    }

    private static void AssertValid<T>(JsonSchema schema, T dto, string contractName)
    {
        EvaluationResults result = Evaluate(schema, dto);

        IEnumerable<string> errors = (result.Details ?? [])
            .Where(detail => detail.Errors != null)
            .SelectMany(detail => detail.Errors!.Select(error => $"{detail.InstanceLocation}: {error.Value}"));
        Assert.True(result.IsValid,
            $"{typeof(T).Name} does not match {contractName}:\n{string.Join("\n", errors)}\n{JsonSerializer.Serialize(dto, WebJson)}");
    }
}
