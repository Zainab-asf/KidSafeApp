namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminNotificationItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = "Safe";
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsNew { get; set; }
}
