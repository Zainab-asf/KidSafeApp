using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminAssignCourseToClassDto
{
    [Required]
    public int CourseId { get; set; }
}
