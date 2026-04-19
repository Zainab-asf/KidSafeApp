namespace KidSafeApp.Shared.DTOs.Courses;

public sealed class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Subject { get; set; }
    public int DifficultyLevel { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CourseLessonDto> Lessons { get; set; } = new();
}
