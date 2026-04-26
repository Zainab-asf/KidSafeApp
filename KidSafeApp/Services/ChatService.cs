using System.Collections.ObjectModel;
using KidSafeApp.Models;
using KidSafeApp.Helpers;
using KidSafeApp.Shared.DTOs.Chat;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

/// <summary>
/// Frontend chat service that communicates with the backend API.
/// Provides methods for sending messages, retrieving history, and managing chats.
/// </summary>
public sealed class ChatService
{
    private readonly MessageApiClient _messageApiClient;

    public ChatService(MessageApiClient messageApiClient)
    {
        _messageApiClient = messageApiClient;
    }

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    /// <summary>
    /// Sends a message to another user via the backend API.
    /// </summary>
    public async Task<MessageDto?> SendAsync(int toUserId, string content, CancellationToken cancellationToken = default)
    {
        if (toUserId <= 0 || string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Invalid recipient or message content.");
        }

        try
        {
            var messageDto = await _messageApiClient.SendMessageAsync(toUserId, content, cancellationToken);
            
            if (messageDto is not null)
            {
                var chatMessage = new ChatMessage
                {
                    Id = new Guid(),
                    SenderId = messageDto.FromUserId.ToString(),
                    RecipientId = messageDto.ToUserId.ToString(),
                    Text = messageDto.Message,
                    SentAt = new DateTimeOffset(messageDto.SentAt)
                };
                Messages.Add(chatMessage);
            }

            return messageDto;
        }
        catch (Exception)
        {
            // Log error - can be connected to ILogger later
            throw;
        }
    }

    /// <summary>
    /// Retrieves message history with another user.
    /// </summary>
    public async Task<IEnumerable<MessageDto>?> GetMessagesAsync(
        int otherUserId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (otherUserId <= 0)
        {
            throw new ArgumentException("Invalid user ID.");
        }

        try
        {
            var messages = await _messageApiClient.GetMessagesAsync(
                otherUserId,
                pageNumber,
                pageSize,
                cancellationToken);

            return messages;
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves chat previews for the current user.
    /// </summary>
    public async Task<IEnumerable<ChatPreviewDto>?> GetChatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await _messageApiClient.GetChatsPreviewAsync(cancellationToken);
            return chats;
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Marks a message as read.
    /// </summary>
    public async Task MarkAsReadAsync(int messageId, CancellationToken cancellationToken = default)
    {
        if (messageId <= 0)
        {
            throw new ArgumentException("Invalid message ID.");
        }

        try
        {
            await _messageApiClient.MarkAsReadAsync(messageId, cancellationToken);
        }
        catch
        {
            throw;
        }
    }
}