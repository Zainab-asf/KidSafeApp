namespace KidSafeApp.Shared.DTOs.Chat
{
    public record MessageDto(int Id, int FromUserId, int ToUserId, string Message, DateTime SentAt);
}
