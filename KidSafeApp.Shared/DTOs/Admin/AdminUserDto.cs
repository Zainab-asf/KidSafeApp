namespace KidSafeApp.Shared.DTOs.Admin;

public sealed record AdminUserDto(
    int Id,
    string Name,
    string Username,
    string Role,
    bool IsApproved,
    bool IsActive,
    DateTime AddedOn
);
