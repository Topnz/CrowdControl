public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public int ParticipantCount { get; set; }

    public Event Event { get; set; }
}