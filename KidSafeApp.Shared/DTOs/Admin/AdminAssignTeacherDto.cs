using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminAssignTeacherDto
{
    [Required]
    public int TeacherId { get; set; }
}
