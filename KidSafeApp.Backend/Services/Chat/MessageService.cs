using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Shared.DTOs.Chat;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Services.Chat;

public sealed class MessageService : IMessageService
{
    private readonly DataContext _dbContext;
    private readonly ILogger<MessageService> _logger;

    public MessageService(DataContext dbContext, ILogger<MessageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<MessageDto> SendMessageAsync(
        int fromUserId, 
        int toUserId, 
        string content, 
        CancellationToken cancellationToken = default)
    {
        if (fromUserId <= 0 || toUserId <= 0 || string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Invalid parameters for sending message.");
        }

        if (!await CanExchangeMessagesAsync(fromUserId, toUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException("Users are not allowed to exchange messages.");
        }

        var message = new Message
        {
            FromId = fromUserId,
            ToId = toUserId,
            Content = content.Trim(),
            SentOn = DateTime.UtcNow
        };

        try
        {
            await _dbContext.Messages.AddAsync(message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Message sent from user {FromUserId} to user {ToUserId}.",
                fromUserId, toUserId);

            return new MessageDto(
                (int)message.Id,
                message.FromId,
                message.ToId,
                message.Content,
                message.SentOn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {FromUserId} to {ToUserId}.", fromUserId, toUserId);
            throw;
        }
    }

    public async Task<IEnumerable<MessageDto>> GetChatMessagesAsync(
        int userId1,
        int userId2,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (userId1 <= 0 || userId2 <= 0)
        {
            throw new ArgumentException("Invalid user IDs.");
        }

        if (!await CanExchangeMessagesAsync(userId1, userId2, cancellationToken))
        {
            return Enumerable.Empty<MessageDto>();
        }

        var skip = (pageNumber - 1) * pageSize;

        try
        {
            var messages = await _dbContext.Messages
                .AsNoTracking()
                .Where(m =>
                    (m.FromId == userId1 && m.ToId == userId2) ||
                    (m.FromId == userId2 && m.ToId == userId1))
                .OrderByDescending(m => m.SentOn)
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new MessageDto(
                    (int)m.Id,
                    m.FromId,
                    m.ToId,
                    m.Content,
                    m.SentOn))
                .ToListAsync(cancellationToken);

            return messages.OrderBy(m => m.SentAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages between {UserId1} and {UserId2}.", userId1, userId2);
            throw;
        }
    }

    public async Task<IEnumerable<ChatPreviewDto>> GetChatsPreviewAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid user ID.");
        }

        try
        {
            var chatPreviews = await _dbContext.Messages
                .AsNoTracking()
                .Where(m => m.FromId == userId || m.ToId == userId)
                .GroupBy(m => m.FromId == userId ? m.ToId : m.FromId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.SentOn).FirstOrDefault()
                })
                .Join(
                    _dbContext.Users.AsNoTracking(),
                    preview => preview.OtherUserId,
                    user => user.Id,
                    (preview, user) => new ChatPreviewDto(
                        user.Id,
                        user.Name,
                        preview.LastMessage != null ? preview.LastMessage.Content : string.Empty,
                        preview.LastMessage == null ? DateTime.UtcNow : preview.LastMessage.SentOn))
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync(cancellationToken);

            return chatPreviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat previews for user {UserId}.", userId);
            throw;
        }
    }

    public async Task MarkMessageAsReadAsync(
        int messageId,
        CancellationToken cancellationToken = default)
    {
        if (messageId <= 0)
        {
            throw new ArgumentException("Invalid message ID.");
        }

        try
        {
            var message = await _dbContext.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

            if (message is null)
            {
                throw new KeyNotFoundException($"Message with ID {messageId} not found.");
            }

            message.IsRead = true;
            _dbContext.Messages.Update(message);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Message {MessageId} marked as read.", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read.", messageId);
            throw;
        }
    }

    public async Task<bool> CanExchangeMessagesAsync(
        int userId1,
        int userId2,
        CancellationToken cancellationToken = default)
    {
        if (userId1 == userId2)
        {
            return false;
        }

        try
        {
            var user1 = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId1 && u.IsActive && u.IsApproved, cancellationToken);

            if (user1 is null)
            {
                return false;
            }

            var otherUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId2 && u.IsActive && u.IsApproved, cancellationToken);

            if (otherUser is null)
            {
                return false;
            }

            // Class-based chat restrictions for child/teacher channels.
            if (string.Equals(user1.Role, "Child", StringComparison.OrdinalIgnoreCase))
            {
                return await HasSharedClassAccessAsync(user1.Id, otherUser.Id, otherUser.Role, cancellationToken);
            }

            if (string.Equals(user1.Role, "Teacher", StringComparison.OrdinalIgnoreCase)
                && string.Equals(otherUser.Role, "Child", StringComparison.OrdinalIgnoreCase))
            {
                return await HasTeacherStudentClassAccessAsync(user1.Id, otherUser.Id, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking message permissions for users {UserId1} and {UserId2}.", userId1, userId2);
            return false;
        }
    }

    private async Task<bool> HasSharedClassAccessAsync(int childId, int otherUserId, string otherUserRole, CancellationToken cancellationToken)
    {
        var childClassIds = await _dbContext.ClassRoomStudents
            .AsNoTracking()
            .Where(cs => cs.StudentId == childId)
            .Select(cs => cs.ClassRoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (childClassIds.Count == 0)
        {
            return false;
        }

        if (string.Equals(otherUserRole, "Child", StringComparison.OrdinalIgnoreCase))
        {
            return await _dbContext.ClassRoomStudents
                .AsNoTracking()
                .AnyAsync(cs => childClassIds.Contains(cs.ClassRoomId) && cs.StudentId == otherUserId, cancellationToken);
        }

        if (string.Equals(otherUserRole, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            return await _dbContext.ClassRooms
                .AsNoTracking()
                .AnyAsync(c => childClassIds.Contains(c.Id) && c.TeacherId == otherUserId, cancellationToken);
        }

        return false;
    }

    private Task<bool> HasTeacherStudentClassAccessAsync(int teacherId, int studentId, CancellationToken cancellationToken)
    {
        return _dbContext.ClassRoomStudents
            .AsNoTracking()
            .Join(
                _dbContext.ClassRooms.AsNoTracking(),
                studentMap => studentMap.ClassRoomId,
                classRoom => classRoom.Id,
                (studentMap, classRoom) => new { studentMap.StudentId, classRoom.TeacherId })
            .AnyAsync(x => x.StudentId == studentId && x.TeacherId == teacherId, cancellationToken);
    }
}
