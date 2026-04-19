namespace KidSafeApp.Shared.DTOs.Courses;

public sealed class CourseLessonDto
{
    public int CourseLessonId { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public int DifficultyLevel { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PdfUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
