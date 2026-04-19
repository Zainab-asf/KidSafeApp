namespace KidSafeApp.Shared.DTOs.Chat
{
    public record MessageDto(int ToUserId, int FromUserId, string Message, DateTime SentOn);
}
