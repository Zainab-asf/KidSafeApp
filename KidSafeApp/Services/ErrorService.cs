using KidSafeApp.Models;
using KidSafeApp.Models.Enums;

namespace KidSafeApp.Services;

/// <summary>
/// Manages application errors and notifications.
/// Provides centralized error handling and user feedback mechanisms.
/// </summary>
public sealed class ErrorService
{
    private readonly List<Notification> _notifications = new();

    /// <summary>
    /// Event raised when the notification collection changes.
    /// </summary>
    public event Action? OnNotificationChanged;

    /// <summary>
    /// Gets the current list of notifications (read-only).
    /// </summary>
    public IReadOnlyList<Notification> Notifications => _notifications.AsReadOnly();

    /// <summary>
    /// Shows an error notification to the user.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    public void ShowError(string message, string? title = null, int durationMs = 5000)
    {
        AddNotification(new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Error,
            Title = title ?? "Error",
            Message = message,
            DurationMs = durationMs
        });
    }

    /// <summary>
    /// Shows a success notification to the user.
    /// </summary>
    /// <param name="message">The success message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    public void ShowSuccess(string message, string? title = null, int durationMs = 3000)
    {
        AddNotification(new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Success,
            Title = title ?? "Success",
            Message = message,
            DurationMs = durationMs
        });
    }

    /// <summary>
    /// Shows a warning notification to the user.
    /// </summary>
    /// <param name="message">The warning message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    public void ShowWarning(string message, string? title = null, int durationMs = 5000)
    {
        AddNotification(new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Warning,
            Title = title ?? "Warning",
            Message = message,
            DurationMs = durationMs
        });
    }

    /// <summary>
    /// Shows an info notification to the user.
    /// </summary>
    /// <param name="message">The info message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    public void ShowInfo(string message, string? title = null, int durationMs = 4000)
    {
        AddNotification(new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Info,
            Title = title ?? "Information",
            Message = message,
            DurationMs = durationMs
        });
    }

    /// <summary>
    /// Removes a notification by ID.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to remove.</param>
    public void RemoveNotification(Guid notificationId)
    {
        _notifications.RemoveAll(n => n.Id == notificationId);
        OnNotificationChanged?.Invoke();
    }

    /// <summary>
    /// Clears all notifications.
    /// </summary>
    public void ClearAll()
    {
        _notifications.Clear();
        OnNotificationChanged?.Invoke();
    }

    private void AddNotification(Notification notification)
    {
        // Limit to 5 notifications at a time to prevent memory buildup
        if (_notifications.Count >= 5)
        {
            _notifications.RemoveAt(0);
        }

        _notifications.Add(notification);
        OnNotificationChanged?.Invoke();
    }
}
