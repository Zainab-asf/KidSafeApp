namespace KidSafe.Backend.Services;

public interface INotificationService
{
    /// <summary>Send alert to a single device token.</summary>
    Task SendFlaggedAlertAsync(string fcmToken, string senderName, string label, string maskedMessage);

    /// <summary>Multicast to all parent/teacher tokens (up to 500 per call).</summary>
    Task BroadcastToParentsAsync(string senderName, string label, string maskedMessage, IReadOnlyList<string> tokens);

    /// <summary>Subscribe a device to a named FCM topic.</summary>
    Task SubscribeToTopicAsync(string fcmToken, string topic);

    /// <summary>Unsubscribe a device from a named FCM topic.</summary>
    Task UnsubscribeFromTopicAsync(string fcmToken, string topic);
}
