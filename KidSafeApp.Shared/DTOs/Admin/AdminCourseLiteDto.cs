namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminCourseLiteDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}
