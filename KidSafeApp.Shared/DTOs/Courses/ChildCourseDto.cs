namespace KidSafeApp.Shared.DTOs.Courses;

public sealed class ChildCourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Subject { get; set; }
    public int DifficultyLevel { get; set; }
    public int LessonCount { get; set; }
    public int CompletedLessons { get; set; }
    public int PercentageComplete { get; set; }
}
