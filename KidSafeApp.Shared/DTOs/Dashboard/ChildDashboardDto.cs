namespace KidSafeApp.Shared.DTOs.Dashboard;

public sealed class ChildDashboardDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Greeting { get; set; } = string.Empty;
    public string ClassLabel { get; set; } = string.Empty;
    public int SafeStreakDays { get; set; }
    public int TotalChats { get; set; }
    public int SafeMessagePercent { get; set; }
    public int BadgesEarned { get; set; }
    public int PointsEarned { get; set; }
    public int PointsGoal { get; set; }
    public int PointsRemaining { get; set; }
    public string? TipTitle { get; set; }
    public string? TipMessage { get; set; }
    public List<ChildBadgeDto> RecentBadges { get; set; } = new();
}
