namespace KidSafe.Backend.Data.Entities;

public class User
{
    public int    Id           { get; set; }
    public string Email        { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Child | Parent | Teacher | Admin</summary>
    public string Role         { get; set; } = "Child";
    /// <summary>active | pending | disabled</summary>
    public string Status       { get; set; } = "active";
    public string DisplayName  { get; set; } = string.Empty;
    public string? AvatarEmoji { get; set; }   // e.g. "😊"
    public string? RollNumber  { get; set; }   // globally unique for Child role
    public string? Phone       { get; set; }
    public string? FcmToken    { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    // navigation
    public ICollection<FlaggedMessage> SentFlaggedMessages { get; set; } = [];
    public ICollection<AbuseReport>    AbuseReports        { get; set; } = [];
    public ICollection<Complaint>      Complaints          { get; set; } = [];
    public ICollection<Notification>   Notifications       { get; set; } = [];
    public ICollection<ClassStudent>   ClassEnrollments    { get; set; } = [];
    public ICollection<Submission>     Submissions         { get; set; } = [];
    public Reward? Reward { get; set; }
}
