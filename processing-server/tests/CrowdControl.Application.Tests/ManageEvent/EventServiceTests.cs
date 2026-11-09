using CrowdControl.Application.ManageEvent;
using CrowdControl.Domain;

namespace CrowdControl.Application.Tests.ManageEvent;

/// <summary>
/// EventService creates and saves events (ID-13) and lists the events that have not ended yet,
/// which GET /events/active returns to ingestion-server.
/// </summary>
public class EventServiceTests
{
    private readonly FakeEventRepository _repository = new FakeEventRepository();
    private readonly EventService _service;

    public EventServiceTests()
    {
        _service = new EventService(_repository);
    }

    [Fact]
    public async Task CreateAsync_ValidValues_SavesEventAndReturnsItWithId()
    {
        DateTime start = DateTime.UtcNow.AddDays(1);

        Event created = await _service.CreateAsync("Roskilde", start, start.AddHours(12), 10, Triangle(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Roskilde", created.Name);
        Event saved = Assert.Single(_repository.Saved);
        Assert.Same(created, saved);
    }

    [Fact]
    public async Task CreateAsync_EndBeforeStart_ThrowsAndSavesNothing()
    {
        DateTime start = DateTime.UtcNow.AddDays(1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync("Roskilde", start, start.AddHours(-1), 10, Triangle(), CancellationToken.None));

        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task CreateAsync_EndInThePast_ThrowsAndSavesNothing()
    {
        DateTime start = DateTime.UtcNow.AddDays(-5);

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync("Roskilde 2020", start, start.AddDays(2), 10, Triangle(), CancellationToken.None));

        Assert.Equal("endTime", exception.ParamName);
        Assert.Empty(_repository.Saved);
    }

    // Creating an event while it is running is allowed; only the end has to be in the future
    [Fact]
    public async Task CreateAsync_StartedButNotEnded_IsSaved()
    {
        DateTime now = DateTime.UtcNow;

        Event created = await _service.CreateAsync("Running", now.AddHours(-1), now.AddHours(1), 10, Triangle(), CancellationToken.None);

        Assert.Same(created, Assert.Single(_repository.Saved));
    }

    [Fact]
    public async Task CreateAsync_TwoPoints_ThrowsAndSavesNothing()
    {
        DateTime start = DateTime.UtcNow.AddDays(1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync("Roskilde", start, start.AddHours(12), 10, Triangle().Take(2), CancellationToken.None));

        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task CreateAsync_CancelledToken_ThrowsAndSavesNothing()
    {
        DateTime start = DateTime.UtcNow.AddDays(1);
        CancellationToken cancelled = new CancellationToken(canceled: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.CreateAsync("Roskilde", start, start.AddHours(12), 10, Triangle(), cancelled));

        Assert.Empty(_repository.Saved);
    }

    // Upcoming and running events are active; ingestion-server must know an event before it starts
    [Fact]
    public async Task GetActiveAsync_ReturnsUpcomingAndRunningButNotEndedEvents()
    {
        DateTime now = DateTime.UtcNow;
        // The service no longer creates ended events, so this one is saved directly
        Event ended = new Event("Ended", now.AddDays(-2), now.AddDays(-1), 10, new Boundary(Triangle()));
        await _repository.SaveAsync(ended, CancellationToken.None);
        Event running = await _service.CreateAsync("Running", now.AddHours(-1), now.AddHours(1), 10, Triangle(), CancellationToken.None);
        Event upcoming = await _service.CreateAsync("Upcoming", now.AddDays(1), now.AddDays(2), 10, Triangle(), CancellationToken.None);

        IReadOnlyList<Event> active = await _service.GetActiveAsync(CancellationToken.None);

        Assert.Equal(2, active.Count);
        Assert.Contains(running, active);
        Assert.Contains(upcoming, active);
        Assert.DoesNotContain(ended, active);
    }

    private static List<GeoPoint> Triangle()
    {
        return new List<GeoPoint>
        {
            new GeoPoint { Latitude = 55.860, Longitude = 9.850 },
            new GeoPoint { Latitude = 55.865, Longitude = 9.850 },
            new GeoPoint { Latitude = 55.865, Longitude = 9.858 }
        };
    }
}
