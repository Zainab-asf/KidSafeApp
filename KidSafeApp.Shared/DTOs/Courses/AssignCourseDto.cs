using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Courses;

public sealed class AssignCourseDto
{
    [Required]
    public int ChildId { get; set; }
}
