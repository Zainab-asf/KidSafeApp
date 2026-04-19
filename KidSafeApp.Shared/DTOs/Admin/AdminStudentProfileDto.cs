namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminStudentProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public DateTime AddedOn { get; set; }
    public List<string> ClassRooms { get; set; } = new();
    public List<AdminCourseLiteDto> Courses { get; set; } = new();
}
