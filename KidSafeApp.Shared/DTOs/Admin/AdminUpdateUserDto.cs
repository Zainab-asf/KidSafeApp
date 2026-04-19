using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminUpdateUserDto
{
    [Required]
    public string Role { get; set; } = "Child";

    public bool IsApproved { get; set; }

    public bool IsActive { get; set; } = true;
}
