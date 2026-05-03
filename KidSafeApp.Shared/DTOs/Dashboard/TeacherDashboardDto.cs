using KidSafeApp.Shared.DTOs.Courses;
using KidSafeApp.Shared.DTOs.Teacher;

namespace KidSafeApp.Shared.DTOs.Dashboard;

public sealed class TeacherDashboardDto
{
    public TeacherSummaryDto Summary { get; set; } = new();
    public List<CourseDto> Courses { get; set; } = new();
    public List<WeeklyPointDto> Weekly { get; set; } = new();
}
