namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminClassRoomDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public List<AdminClassRoomStudentDto> Students { get; set; } = new();
    public List<AdminCourseLiteDto> Courses { get; set; } = new();
}
