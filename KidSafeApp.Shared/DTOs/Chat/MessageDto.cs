namespace KidSafeApp.Shared.DTOs.Chat
{
    public record MessageDto(
        int Id,
        int FromUserId,
        int ToUserId,
        string Message,
        DateTime SentAt,
        bool IsFlagged = false,
        string? FlagReason = null,
        string? FromUserName = null,
        string? FromUserAvatarUrl = null);
}
