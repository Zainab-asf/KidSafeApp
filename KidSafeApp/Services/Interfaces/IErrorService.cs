using KidSafeApp.Models;

namespace KidSafeApp.Services.Interfaces;

/// <summary>
/// Provides centralized error handling and user notification mechanisms.
/// </summary>
public interface IErrorService
{
    /// <summary>
    /// Event raised when the notification collection changes.
    /// </summary>
    event Action? OnNotificationChanged;

    /// <summary>
    /// Gets the current list of notifications (read-only).
    /// </summary>
    IReadOnlyList<Notification> Notifications { get; }

    /// <summary>
    /// Shows an error notification to the user.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    void ShowError(string message, string? title = null, int durationMs = 5000);

    /// <summary>
    /// Shows a success notification to the user.
    /// </summary>
    /// <param name="message">The success message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    void ShowSuccess(string message, string? title = null, int durationMs = 3000);

    /// <summary>
    /// Shows a warning notification to the user.
    /// </summary>
    /// <param name="message">The warning message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    void ShowWarning(string message, string? title = null, int durationMs = 5000);

    /// <summary>
    /// Shows an info notification to the user.
    /// </summary>
    /// <param name="message">The info message to display.</param>
    /// <param name="title">Optional title for the notification.</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismiss.</param>
    void ShowInfo(string message, string? title = null, int durationMs = 4000);

    /// <summary>
    /// Removes a notification by ID.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to remove.</param>
    void RemoveNotification(Guid notificationId);

    /// <summary>
    /// Clears all notifications.
    /// </summary>
    void ClearAll();
}