namespace KidSafeApp.Models.Enums;

/// <summary>
/// Represents the type of notification to display to the user.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Indicates an error notification.
    /// </summary>
    Error,

    /// <summary>
    /// Indicates a success notification.
    /// </summary>
    Success,

    /// <summary>
    /// Indicates a warning notification.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates an informational notification.
    /// </summary>
    Info
}