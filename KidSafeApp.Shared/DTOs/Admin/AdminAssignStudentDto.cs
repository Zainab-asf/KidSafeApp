using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminAssignStudentDto
{
    [Required]
    public int StudentId { get; set; }
}
