using CrowdControl.Application.MonitorEvent;

namespace CrowdControl.Application.Tests;

public class GridAggregatorTests
{
    [Fact]
    public void Add_TwoPositionsInSameCell_GivesOneCellWithCountTwo()
    {
        GridAggregator aggregator = new GridAggregator(CreateTestEvent());

        aggregator.Add(0.0011, 0.0011);
        aggregator.Add(0.00115, 0.00115);

        HeatmapSnapshot snapshot = aggregator.GetSnapshot();
        GridCell cell = Assert.Single(snapshot.Cells);
        Assert.Equal(2, cell.Count);
    }

    [Fact]
    public void Add_TwoPositionsFarApart_GivesTwoCells()
    {
        GridAggregator aggregator = new GridAggregator(CreateTestEvent());

        aggregator.Add(0.0011, 0.0011);
        aggregator.Add(0.0091, 0.0091);

        HeatmapSnapshot snapshot = aggregator.GetSnapshot();
        Assert.Equal(2, snapshot.Cells.Count);
    }

    [Fact]
    public void Add_PositionInsideZone_CountsInZone()
    {
        GridAggregator aggregator = new GridAggregator(CreateTestEvent());

        aggregator.Add(0.0011, 0.0011);

        HeatmapSnapshot snapshot = aggregator.GetSnapshot();
        ZoneCount zone = Assert.Single(snapshot.Zones);
        Assert.Equal(1, zone.Count);
    }

    [Fact]
    public void Add_PositionOutsideZone_DoesNotCountInZone()
    {
        GridAggregator aggregator = new GridAggregator(CreateTestEvent());

        aggregator.Add(0.0091, 0.0091);

        HeatmapSnapshot snapshot = aggregator.GetSnapshot();
        ZoneCount zone = Assert.Single(snapshot.Zones);
        Assert.Equal(0, zone.Count);
    }

    [Fact]
    public void Reset_ClearsCellsAndZoneCounts()
    {
        GridAggregator aggregator = new GridAggregator(CreateTestEvent());
        aggregator.Add(0.0011, 0.0011);

        aggregator.Reset();

        HeatmapSnapshot snapshot = aggregator.GetSnapshot();
        Assert.Empty(snapshot.Cells);
        ZoneCount zone = Assert.Single(snapshot.Zones);
        Assert.Equal(0, zone.Count);
    }

    [Fact]
    public void GetSnapshot_ZoneWithoutCapacity_GivesNullCapacity()
    {
        Event ev = CreateTestEvent();
        ev.Zones[0].Capacity = null;
        GridAggregator aggregator = new GridAggregator(ev);

        HeatmapSnapshot snapshot = aggregator.GetSnapshot();

        ZoneCount zone = Assert.Single(snapshot.Zones);
        Assert.Null(zone.Capacity);
    }

    // Event fra (0, 0) til (0.01, 0.01) med én zone i det nederste venstre hjørne
    private static Event CreateTestEvent()
    {
        return new Event
        {
            Name = "Test-event",
            SendInterval = 10,
            Perimiter = Rectangle(0, 0.01, 0, 0.01),
            Zones = new List<Zone>
            {
                new Zone { Name = "Test-zone", Capacity = 10, Boundary = Rectangle(0, 0.005, 0, 0.005) }
            }
        };
    }

    private static Boundary Rectangle(double south, double north, double west, double east)
    {
        Boundary boundary = new Boundary();
        boundary.AddGeoPoints(new List<GeoPoint>
        {
            new GeoPoint { Latitude = south, Longitude = west },
            new GeoPoint { Latitude = south, Longitude = east },
            new GeoPoint { Latitude = north, Longitude = east },
            new GeoPoint { Latitude = north, Longitude = west }
        });
        return boundary;
    }
}
