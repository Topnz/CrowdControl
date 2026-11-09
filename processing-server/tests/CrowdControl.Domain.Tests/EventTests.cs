namespace CrowdControl.Domain.Tests;

/// <summary>
/// An Event validates itself on construction (ID-13), so an invalid event can never exist.
/// </summary>
public class EventTests
{
    private static readonly DateTime Start = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = Start.AddHours(12);

    [Fact]
    public void Constructor_ValidValues_CreatesEventWithoutId()
    {
        Event ev = new Event("Roskilde", Start, End, 10, Triangle());

        Assert.Equal("Roskilde", ev.Name);
        Assert.Equal(Start, ev.StartTime);
        Assert.Equal(End, ev.EndTime);
        Assert.Equal(10, ev.SendInterval);
        Assert.Equal(3, ev.Perimeter.GeoPoints.Count);
        Assert.Empty(ev.Zones);
        // The repository assigns the ID when the event is saved
        Assert.Equal(Guid.Empty, ev.Id);
    }

    [Fact]
    public void Constructor_EndBeforeStart_Throws()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new Event("Roskilde", Start, Start.AddHours(-1), 10, Triangle()));

        Assert.Equal("endTime", exception.ParamName);
    }

    [Fact]
    public void Constructor_EndEqualToStart_Throws()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new Event("Roskilde", Start, Start, 10, Triangle()));

        Assert.Equal("endTime", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyName_Throws(string? name)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new Event(name!, Start, End, 10, Triangle()));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_SendIntervalBelowOne_Throws(int sendInterval)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new Event("Roskilde", Start, End, sendInterval, Triangle()));

        Assert.Equal("sendInterval", exception.ParamName);
    }

    [Fact]
    public void Constructor_NoPerimeter_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Event("Roskilde", Start, End, 10, null!));
    }

    [Fact]
    public void AssignId_FirstTime_SetsId()
    {
        Event ev = new Event("Roskilde", Start, End, 10, Triangle());
        Guid id = Guid.NewGuid();

        ev.AssignId(id);

        Assert.Equal(id, ev.Id);
    }

    [Fact]
    public void AssignId_Twice_Throws()
    {
        Event ev = new Event("Roskilde", Start, End, 10, Triangle());
        ev.AssignId(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => ev.AssignId(Guid.NewGuid()));
    }

    private static Boundary Triangle()
    {
        return new Boundary(new List<GeoPoint>
        {
            new GeoPoint { Latitude = 55.860, Longitude = 9.850 },
            new GeoPoint { Latitude = 55.865, Longitude = 9.850 },
            new GeoPoint { Latitude = 55.865, Longitude = 9.858 }
        });
    }
}
