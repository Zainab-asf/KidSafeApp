namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminParentPortalRowDto
{
    public int ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ChildName { get; set; } = string.Empty;
    public string ChildGrade { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? LastActiveAt { get; set; }
    public int PendingAlerts { get; set; }
}
