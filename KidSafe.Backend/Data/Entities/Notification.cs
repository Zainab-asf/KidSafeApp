namespace KidSafe.Backend.Data.Entities;

public class Notification
{
    public int      Id        { get; set; }
    public int      UserId    { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string   Body      { get; set; } = string.Empty;
    /// <summary>alert | assignment | badge | system</summary>
    public string   Type      { get; set; } = "system";
    public bool     IsRead    { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
