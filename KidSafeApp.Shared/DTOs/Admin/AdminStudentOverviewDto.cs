namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminStudentOverviewDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public List<string> ClassRooms { get; set; } = new();
}
