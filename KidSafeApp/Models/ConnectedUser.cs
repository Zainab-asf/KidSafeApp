namespace KidSafeApp.Models;

/// <summary>
/// Represents a user currently connected to the chat system.
/// </summary>
public sealed class ConnectedUser
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsOnline { get; init; }
}
