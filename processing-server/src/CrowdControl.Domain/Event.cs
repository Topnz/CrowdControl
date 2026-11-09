namespace CrowdControl.Domain;

public class Event
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int SendInterval { get; private set; }
    public Boundary Perimeter { get; private set; }
    private List<Zone> _zones = [];
    public IReadOnlyList<Zone> Zones => _zones.AsReadOnly();

    public Event(string name, DateTime startTime, DateTime endTime,
        int sendInterval, Boundary perimeter)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Event name must not be empty", nameof(name));
        }
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time", nameof(endTime));
        }
        if (sendInterval <= 0)
        {
            throw new ArgumentException("Send interval must be greater than 0", nameof(sendInterval));
        }
        ArgumentNullException.ThrowIfNull(perimeter);
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
        SendInterval = sendInterval;
        Perimeter = perimeter;
    }
    
    public void AssignId(Guid id)
    {
        if (Id != Guid.Empty)
        {
            throw new InvalidOperationException("Id has already been assigned");
        }
        Id = id;
    }
    
    public bool AddZone(Zone zone)
    {
        // TODO (ID-25): check that the zone lies inside the perimeter and does not overlap other zones
        if (true)
        {
            _zones.Add(zone);
            return true;
        }
        else
        {
            return false;
        }
    }


}