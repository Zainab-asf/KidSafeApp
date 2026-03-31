namespace KidSafeApp.Models;

/// <summary>
/// Represents a single chat message exchanged between users.
/// </summary>
public sealed class ChatMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string SenderId { get; init; } = string.Empty;

    public string SenderDisplayName { get; init; } = string.Empty;

    public string? RecipientId { get; init; }
    public string Text { get; init; } = string.Empty;

    public DateTimeOffset SentAt { get; init; } = DateTimeOffset.UtcNow;

    public bool IsFlaggedAsBullying { get; init; }

    public string? FlagReason { get; init; }
}
