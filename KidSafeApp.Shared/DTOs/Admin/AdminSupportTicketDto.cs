namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminSupportTicketDto
{
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Low";
    public DateTime CreatedAt { get; set; }
}
