namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminChildChatStudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string GradeLabel { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Safe";
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public string? LastMessagePreview { get; set; }
}
