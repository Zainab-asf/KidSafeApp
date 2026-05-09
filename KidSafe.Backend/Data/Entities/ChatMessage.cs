namespace KidSafe.Backend.Data.Entities;

/// <summary>Persisted group chat message for a class room.</summary>
public class ChatMessage
{
    public int      Id        { get; set; }
    public int      ClassId   { get; set; }
    public int      SenderId  { get; set; }
    public string   Content   { get; set; } = string.Empty;
    public string   Label     { get; set; } = "Safe";   // Safe | Watch | Review
    public double   Score     { get; set; }
    public string?  Masked    { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Class Class  { get; set; } = null!;
    public User  Sender { get; set; } = null!;
}
