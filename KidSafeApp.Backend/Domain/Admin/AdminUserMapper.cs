using KidSafeApp.Backend.Data.Entities;

namespace KidSafeApp.Backend.Domain.Admin;

public static class AdminUserMapper
{
    public static AdminUserDto ToDto(User user) =>
        new(
            user.Id,
            user.Name,
            user.Username,
            user.Role,
            user.IsApproved,
            user.IsActive,
            user.AddedOn
        );
}
