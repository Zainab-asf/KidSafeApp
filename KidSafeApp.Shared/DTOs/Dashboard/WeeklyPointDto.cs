namespace KidSafeApp.Shared.DTOs.Dashboard;

public sealed class WeeklyPointDto
{
    public string Day { get; set; } = string.Empty;
    public int Safe { get; set; }
    public int Flagged { get; set; }
    public int Blocked { get; set; }
}
