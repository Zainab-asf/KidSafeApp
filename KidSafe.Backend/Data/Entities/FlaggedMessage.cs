namespace KidSafe.Backend.Data.Entities;

public class FlaggedMessage
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string MaskedMessage { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty; // flagged | blocked
    public double Score { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public User Sender { get; set; } = null!;
}
