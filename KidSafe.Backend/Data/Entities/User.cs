namespace KidSafe.Backend.Data.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Child | Parent | Teacher | Admin</summary>
    public string Role { get; set; } = "Child";
    /// <summary>active | pending | disabled</summary>
    public string Status { get; set; } = "active";
    public string DisplayName { get; set; } = string.Empty;
    public string? FcmToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FlaggedMessage> SentFlaggedMessages { get; set; } = new List<FlaggedMessage>();
    public ICollection<AbuseReport> AbuseReports { get; set; } = new List<AbuseReport>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public Reward? Reward { get; set; }
}
