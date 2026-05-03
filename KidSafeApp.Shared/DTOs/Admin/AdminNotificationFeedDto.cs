namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminNotificationFeedDto
{
    public List<AdminNotificationItemDto> Items { get; set; } = new();
}
