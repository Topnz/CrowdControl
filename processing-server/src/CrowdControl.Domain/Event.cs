using System.IO.Compression;

public class Event
{
    public Guid Id { get; private set; }
    public required string Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public required int SendInterval { get; set; }
    public required Boundary Perimiter { get; set; }
    public List<Zone> Zones { get; set;}

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
        if(/*logic to check if zone is inside perimiter, and not any other zones*/ true)
        {
            Zones.Add(zone);
            return true;
        }
        else
        {
            return false;
        }
    }


}