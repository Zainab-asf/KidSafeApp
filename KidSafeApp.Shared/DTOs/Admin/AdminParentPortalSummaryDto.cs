namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminParentPortalSummaryDto
{
    public int TotalParents { get; set; }
    public int ActiveThisWeek { get; set; }
    public int PendingAlerts { get; set; }
    public List<AdminParentPortalRowDto> Parents { get; set; } = new();
}
