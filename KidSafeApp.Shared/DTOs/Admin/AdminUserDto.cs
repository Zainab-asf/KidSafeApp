namespace KidSafeApp.Shared.DTOs.Admin;

public sealed class AdminUserDto
{
    public AdminUserDto() { }

    public AdminUserDto(int id, string name, string username, string role, bool isApproved, bool isActive, DateTime addedOn)
    {
        Id = id;
        Name = name;
        Username = username;
        Role = role;
        IsApproved = isApproved;
        IsActive = isActive;
        AddedOn = addedOn;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public DateTime AddedOn { get; set; }

    // Optional enrichment fields (not always populated)
    public string? ClassName { get; set; }
    public string? ParentName { get; set; }
    public string? LinkedChildName { get; set; }
}
