using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Courses;

public sealed class AddCourseLessonDto
{
    [Required]
    public int LessonId { get; set; }

    [Range(1, 9999)]
    public int SortOrder { get; set; } = 1;
}
