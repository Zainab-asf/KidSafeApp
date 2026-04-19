namespace KidSafeApp.Shared.DTOs.Teacher;

public sealed class TeacherSummaryDto
{
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int TotalLessons { get; set; }
    public int PublishedLessons { get; set; }
    public int CompletedLessonSubmissions { get; set; }
    public int PendingLessonSubmissions { get; set; }
    public double AverageProgressPercent { get; set; }
}
