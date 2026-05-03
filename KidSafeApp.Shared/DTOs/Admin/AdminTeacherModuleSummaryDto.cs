namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminTeacherModuleSummaryDto
{
    public int TotalTeachers { get; set; }
    public int ActiveTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalAlerts { get; set; }
    public List<AdminTeacherRowDto> Teachers { get; set; } = new();
}
