namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminHelpItemDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
