namespace KidSafeApp.Shared.DTOs;

public sealed class LessonProgressDto
{
    public int CourseLessonId { get; set; }
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public int PercentageComplete { get; set; }
    public int TimeSpentSeconds { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
}
