namespace KidSafe.Backend.Data.Entities;

public class ModerationAction
{
    public int Id { get; set; }
    /// <summary>mute | block | warn | removeContent</summary>
    public string Type { get; set; } = string.Empty;
    public int ActorId { get; set; }        // Admin or Teacher
    public int TargetUserId { get; set; }
    public int? FlaggedMessageId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public User Actor { get; set; } = null!;
    public User TargetUser { get; set; } = null!;
}
