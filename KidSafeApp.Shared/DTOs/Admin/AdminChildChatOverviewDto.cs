namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminChildChatOverviewDto
{
    public int TotalStudents { get; set; }
    public int ActiveChats { get; set; }
    public int FlaggedToday { get; set; }
    public int Blocked { get; set; }
    public List<AdminChildChatStudentDto> Students { get; set; } = new();
}
