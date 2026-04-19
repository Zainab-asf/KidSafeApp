using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Progress;

public sealed class UpdateLessonProgressDto
{
    [Required]
    public int CourseLessonId { get; set; }

    [Range(0, 100)]
    public int PercentageComplete { get; set; }

    public int TimeSpentSeconds { get; set; }

    public bool IsCompleted { get; set; }
}
