namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminClassRoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}
