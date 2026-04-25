namespace KidSafeApp.Models;

using KidSafeApp.Services;

public class ErrorNotification
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int DurationMs { get; set; } = 5000;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
