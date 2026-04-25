using KidSafeApp.Models;

namespace KidSafeApp.Services;

/// <summary>
/// Manages application errors and notifications.
/// Provides centralized error handling and user feedback mechanisms.
/// </summary>
public sealed class ErrorService
{
    private List<ErrorNotification> _notifications = new();
    public event Action? OnNotificationChanged;

    public IReadOnlyList<ErrorNotification> Notifications => _notifications.AsReadOnly();

    /// <summary>
    /// Shows an error notification to the user.
    /// </summary>
    public void ShowError(string message, string? title = null, int durationMs = 5000)
    {
        AddNotification(new ErrorNotification
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
    public void ShowSuccess(string message, string? title = null, int durationMs = 3000)
    {
        AddNotification(new ErrorNotification
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
    public void ShowWarning(string message, string? title = null, int durationMs = 5000)
    {
        AddNotification(new ErrorNotification
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
    public void ShowInfo(string message, string? title = null, int durationMs = 4000)
    {
        AddNotification(new ErrorNotification
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

    private void AddNotification(ErrorNotification notification)
    {
        // Limit to 5 notifications at a time
        if (_notifications.Count >= 5)
        {
            _notifications.RemoveAt(0);
        }

        _notifications.Add(notification);
        OnNotificationChanged?.Invoke();
    }
}

public enum NotificationType
{
    Error,
    Success,
    Warning,
    Info
}
