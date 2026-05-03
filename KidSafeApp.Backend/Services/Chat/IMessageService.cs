using KidSafeApp.Shared.DTOs.Chat;

namespace KidSafeApp.Backend.Services.Chat;

public interface IMessageService
{
    /// <summary>
    /// Sends a message from one user to another and persists it to the database.
    /// </summary>
    Task<MessageDto> SendMessageAsync(int fromUserId, int toUserId, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the message history between two users with pagination support.
    /// </summary>
    Task<IEnumerable<MessageDto>> GetChatMessagesAsync(
        int userId1, 
        int userId2, 
        int pageNumber = 1, 
        int pageSize = 50, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a preview of all active chats for a user (most recent message per chat).
    /// </summary>
    Task<IEnumerable<ChatPreviewDto>> GetChatsPreviewAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as read.
    /// </summary>
    Task MarkMessageAsReadAsync(int messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether two users are allowed to exchange messages.
    /// </summary>
    Task<bool> CanExchangeMessagesAsync(int userId1, int userId2, CancellationToken cancellationToken = default);
}
