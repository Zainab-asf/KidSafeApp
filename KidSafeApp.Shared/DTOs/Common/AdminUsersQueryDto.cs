namespace KidSafeApp.Shared.DTOs.Common;

public sealed class AdminUsersQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsApproved { get; set; }
}

