using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminCreateUserDto
{
    [Required, MaxLength(25)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Child";

    [MaxLength(50)]
    public string? RegistrationNo { get; set; }

    public int? ClassRoomId { get; set; }

    public int? CourseId { get; set; }

    public bool IsApproved { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public int? ClassRoomId { get; set; }

    public int? CourseId { get; set; }
}
