namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminTeacherRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public List<string> Classes { get; set; } = new();
    public int StudentCount { get; set; }
    public int AlertCount { get; set; }
    public string Status { get; set; } = "Active";
}
