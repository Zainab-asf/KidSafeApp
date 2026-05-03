namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminChatLineDto
{
    public int FromUserId { get; set; }
    public string FromName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsAlert { get; set; }
}
