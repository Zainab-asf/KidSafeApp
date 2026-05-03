namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminAlertDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Safe";
    public DateTime CreatedAt { get; set; }
}
