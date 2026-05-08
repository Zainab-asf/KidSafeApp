using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace KidSafe.Backend.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly bool _enabled;

    // FCM topic all parents/teachers subscribe to
    public const string ParentsTopic = "kidsafe-parents";

    public NotificationService(ILogger<NotificationService> logger, IConfiguration config)
    {
        _logger = logger;
        var credFile = config["Firebase:CredentialFile"];

        if (FirebaseApp.DefaultInstance == null)
        {
            if (!string.IsNullOrEmpty(credFile) && File.Exists(credFile))
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credFile)
                });
                _enabled = true;
                _logger.LogInformation("Firebase initialized from {File}", credFile);
            }
            else
            {
                _logger.LogWarning(
                    "Firebase credentials not found at '{File}'. " +
                    "Push notifications are disabled. " +
                    "Set Firebase:CredentialFile in appsettings.json to enable.",
                    credFile);
                _enabled = false;
            }
        }
        else
        {
            _enabled = true;
        }
    }

    // ── single-device ─────────────────────────────────────────────────────────

    public async Task SendFlaggedAlertAsync(
        string fcmToken, string senderName, string label, string maskedMessage)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(fcmToken)) return;

        var msg = BuildMessage(fcmToken, senderName, label, maskedMessage);
        await SendWithRetryAsync(() => FirebaseMessaging.DefaultInstance.SendAsync(msg), fcmToken);
    }

    // ── multicast (up to 500 tokens) ──────────────────────────────────────────

    public async Task BroadcastToParentsAsync(
        string senderName, string label, string maskedMessage, IReadOnlyList<string> tokens)
    {
        if (!_enabled || tokens.Count == 0) return;

        // FCM multicast limit is 500 tokens per call
        foreach (var batch in tokens.Chunk(500))
        {
            var msg = new MulticastMessage
            {
                Tokens = batch.ToList(),
                Notification = BuildNotification(label, senderName, maskedMessage),
                Data = BuildData(label, senderName, maskedMessage),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "kidsafe_alerts",
                        Color = label == "blocked" ? "#ef4444" : "#f59e0b",
                        Sound = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Sound = "default", Badge = 1 }
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(msg);
                _logger.LogInformation(
                    "Multicast: {Success}/{Total} delivered (label={Label})",
                    response.SuccessCount, batch.Length, label);

                // Log individual failures for debugging
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                        _logger.LogWarning("Token {Token} failed: {Error}",
                            batch[i][..10] + "…", response.Responses[i].Exception?.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Multicast FCM failed");
            }
        }
    }

    // ── topic management ──────────────────────────────────────────────────────

    public async Task SubscribeToTopicAsync(string fcmToken, string topic)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(fcmToken)) return;
        try
        {
            var response = await FirebaseMessaging.DefaultInstance
                .SubscribeToTopicAsync(new[] { fcmToken }, topic);
            _logger.LogInformation("Subscribed to topic '{Topic}': {Success} success", topic, response.SuccessCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Topic subscribe failed for topic '{Topic}'", topic);
        }
    }

    public async Task UnsubscribeFromTopicAsync(string fcmToken, string topic)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(fcmToken)) return;
        try
        {
            await FirebaseMessaging.DefaultInstance
                .UnsubscribeFromTopicAsync(new[] { fcmToken }, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Topic unsubscribe failed for topic '{Topic}'", topic);
        }
    }

    // ── retry helper (handles token expiry) ──────────────────────────────────

    private async Task SendWithRetryAsync(Func<Task<string>> sendFunc, string token)
    {
        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var msgId = await sendFunc();
                _logger.LogDebug("FCM sent: {MessageId}", msgId);
                return;
            }
            catch (FirebaseMessagingException fex) when (
                fex.MessagingErrorCode is MessagingErrorCode.Unregistered
                                       or MessagingErrorCode.InvalidArgument)
            {
                // Token is stale — no point retrying
                _logger.LogWarning("FCM token stale/invalid ({Token}), skipping", token[..10] + "…");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning("FCM attempt {Attempt} failed, retrying… ({Error})", attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FCM failed after {Max} attempts", maxAttempts);
            }
        }
    }

    // ── builders ──────────────────────────────────────────────────────────────

    private static Message BuildMessage(string token, string senderName, string label, string maskedMessage) =>
        new()
        {
            Token        = token,
            Notification = BuildNotification(label, senderName, maskedMessage),
            Data         = BuildData(label, senderName, maskedMessage),
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "kidsafe_alerts",
                    Color     = label == "blocked" ? "#ef4444" : "#f59e0b",
                    Sound     = "default"
                }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps { Sound = "default", Badge = 1 }
            },
            Webpush = new WebpushConfig
            {
                Notification = new WebpushNotification
                {
                    Title = label == "blocked" ? "🚨 Message Blocked" : "⚠️ Message Flagged",
                    Body  = $"{senderName}: {maskedMessage}",
                    Icon  = "/icon-192.png"
                }
            }
        };

    private static Notification BuildNotification(string label, string senderName, string maskedMessage) =>
        new()
        {
            Title = label == "blocked" ? "🚨 Message Blocked" : "⚠️ Message Flagged",
            Body  = $"{senderName} sent a {label} message: {maskedMessage}"
        };

    private static Dictionary<string, string> BuildData(string label, string senderName, string maskedMessage) =>
        new()
        {
            ["label"]        = label,
            ["sender"]       = senderName,
            ["message"]      = maskedMessage,
            ["timestamp"]    = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            ["click_action"] = "FLUTTER_NOTIFICATION_CLICK"
        };
}
