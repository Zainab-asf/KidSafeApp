namespace KidSafeApp.Constants;

/// <summary>
/// Constants for SignalR hub communication.
/// </summary>
public static class HubConstants
{
    /// <summary>
    /// The hub route path.
    /// </summary>
    public const string HubUrl = "hubs/kidsafeapp";

    /// <summary>
    /// Server-to-client method names.
    /// </summary>
    public static class ServerMethods
    {
        public const string UserConnected = "UserConnected";
        public const string OnlineUsersList = "OnlineUsersList";
        public const string UserIsOnline = "UserIsOnline";
        public const string MessageReceived = "MessageRecieved"; // Note: keeping original typo for compatibility
        public const string RosterUpdated = "RosterUpdated";
    }

    /// <summary>
    /// Client-to-server method names.
    /// </summary>
    public static class ClientMethods
    {
        public const string SetUserOnline = "SetUserOnline";
    }
}