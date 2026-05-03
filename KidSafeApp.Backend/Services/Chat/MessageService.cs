using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Shared.DTOs.Chat;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Services.Chat;

public sealed class MessageService : IMessageService
{
    private readonly DataContext _db;

    public MessageService(DataContext db)
    {
        _db = db;
    }

    public async Task<MessageDto> SendMessageAsync(int fromUserId, int toUserId, string content, CancellationToken cancellationToken)
    {
        if (fromUserId <= 0 || toUserId <= 0 || string.IsNullOrWhiteSpace(content))
        {
            throw new ServiceException("Invalid message.", StatusCodes.Status400BadRequest);
        }

        var fromExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == fromUserId, cancellationToken);
        var toExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == toUserId, cancellationToken);
        if (!fromExists || !toExists)
        {
            throw new ServiceException("User not found.", StatusCodes.Status404NotFound);
        }

        var message = new Message
        {
            FromId = fromUserId,
            ToId = toUserId,
            Content = content.Trim(),
            SentOn = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        return new MessageDto(message.ToId, message.FromId, message.Content, message.SentOn);
    }
}

