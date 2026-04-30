using KidSafeApp.Models.Enums;

namespace KidSafeApp.Models;

/// <summary>
/// Represents a notification to display to the user.
/// </summary>
public class Notification
{
    /// <summary>
    /// Unique identifier for the notification.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The type of notification (Error, Success, Warning, Info).
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// The title of the notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The message content of the notification.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Duration in milliseconds before the notification auto-dismisses.
    /// </summary>
    public int DurationMs { get; set; } = 5000;

    /// <summary>
    /// When the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
