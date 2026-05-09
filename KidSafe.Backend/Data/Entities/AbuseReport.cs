namespace KidSafe.Backend.Data.Entities;

public class AbuseReport
{
    public int Id { get; set; }
    public int ReporterId { get; set; }
    public int? ReferencedMessageId { get; set; }  // FK → FlaggedMessage
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public User Reporter { get; set; } = null!;
}
