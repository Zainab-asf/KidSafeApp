using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminCreateClassRoomDto
{
    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Grade { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Section { get; set; } = string.Empty;
}
