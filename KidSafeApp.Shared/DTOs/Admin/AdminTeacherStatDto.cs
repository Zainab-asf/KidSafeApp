namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminTeacherStatDto
{
    public int TeacherId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int ClassCount { get; set; }
}
