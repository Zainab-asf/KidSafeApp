using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Courses;

public sealed class CreateCourseDto
{
    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(60)]
    public string? Subject { get; set; }

    [Range(1, 5)]
    public int DifficultyLevel { get; set; } = 1;
}
