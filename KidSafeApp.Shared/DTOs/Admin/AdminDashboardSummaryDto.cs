namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminDashboardSummaryDto
{
    public int ActiveStudents { get; set; }
    public int ChatsMonitored { get; set; }
    public int FlaggedMessages { get; set; }
    public int BlockedUsers { get; set; }

    public int SafetyScore { get; set; }

    public List<AdminWeeklyActivityDto> Weekly { get; set; } = new();
    public List<AdminAlertDto> RecentAlerts { get; set; } = new();
    public List<AdminTeacherStatDto> TopTeachers { get; set; } = new();
}

public sealed class AdminWeeklyActivityDto
{
    public string Day { get; set; } = string.Empty;
    public int Safe { get; set; }
    public int Flagged { get; set; }
    public int Blocked { get; set; }
}

public sealed class AdminAlertDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Safe";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class AdminTeacherStatDto
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int ClassCount { get; set; }
}

