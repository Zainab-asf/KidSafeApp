using KidSafeApp.Shared.DTOs.Dashboard;

namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminDashboardSummaryDto
{
    public int ActiveStudents { get; set; }
    public int ChatsMonitored { get; set; }
    public int FlaggedMessages { get; set; }
    public int BlockedUsers { get; set; }
    public int SafetyScore { get; set; }
    public List<WeeklyPointDto> Weekly { get; set; } = new();
    public List<AdminAlertDto> RecentAlerts { get; set; } = new();
    public List<AdminTeacherStatDto> TopTeachers { get; set; } = new();
}
