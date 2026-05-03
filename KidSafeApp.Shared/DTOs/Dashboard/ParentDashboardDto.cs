using KidSafeApp.Shared.DTOs.Notifications;

namespace KidSafeApp.Shared.DTOs.Dashboard;

public sealed class ParentDashboardDto
{
    public string ParentName { get; set; } = string.Empty;
    public string ChildName { get; set; } = string.Empty;
    public string ChildGrade { get; set; } = string.Empty;
    public bool HasLinkedChild { get; set; }
    public int SafeRate { get; set; }
    public int FlaggedCount { get; set; }
    public int UnreadAlerts { get; set; }
    public int BadgesEarned { get; set; }
    public int SafeMessages { get; set; }
    public int SafeBar { get; set; }
    public int FlaggedBar { get; set; }
    public int BadgeBar { get; set; }
    public int SafeStreak { get; set; }
    public int NewFlags { get; set; }
    public List<WeeklyPointDto> Weekly { get; set; } = new();
    public List<NotificationDto> RecentAlerts { get; set; } = new();
}
