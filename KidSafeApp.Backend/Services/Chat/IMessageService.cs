using KidSafeApp.Shared.DTOs.Chat;

namespace KidSafeApp.Backend.Services.Chat;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(int fromUserId, int toUserId, string content, CancellationToken cancellationToken);
}

