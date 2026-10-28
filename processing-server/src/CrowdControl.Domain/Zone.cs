public class Zone
{
    public string Name { get; set; }
    public int? Capacity { get; set; } // null when the zone has no capacity

    public Boundary Boundary {get; set;}
}