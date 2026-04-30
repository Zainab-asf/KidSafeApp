using System.Collections.ObjectModel;
using KidSafeApp.Models;
using KidSafeApp.Shared.DTOs.Chat;

namespace KidSafeApp.Services.Interfaces;

/// <summary>
/// Provides chat messaging functionality.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Gets the collection of chat messages.
    /// </summary>
    ObservableCollection<ChatMessage> Messages { get; }

    /// <summary>
    /// Sends a message to another user.
    /// </summary>
    /// <param name="toUserId">The recipient's user ID.</param>
    /// <param name="content">The message content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sent message DTO, or null if failed.</returns>
    Task<MessageDto?> SendAsync(int toUserId, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves message history with another user.
    /// </summary>
    /// <param name="otherUserId">The other user's ID.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of messages per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of message DTOs.</returns>
    Task<IEnumerable<MessageDto>?> GetMessagesAsync(
        int otherUserId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves chat previews for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of chat preview DTOs.</returns>
    Task<IEnumerable<ChatPreviewDto>?> GetChatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as read.
    /// </summary>
    /// <param name="messageId">The message ID to mark as read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkAsReadAsync(int messageId, CancellationToken cancellationToken = default);
}