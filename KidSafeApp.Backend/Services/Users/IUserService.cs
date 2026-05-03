using KidSafeApp.Shared.DTOs.Admin;
using KidSafeApp.Shared.DTOs.Common;

namespace KidSafeApp.Backend.Services.Users;

public interface IUserService
{
    Task<PagedResultDto<AdminUserDto>> GetUsersAsync(AdminUsersQueryDto query, CancellationToken cancellationToken);
    Task<AdminUserDto> CreateUserAsync(AdminCreateUserDto dto, CancellationToken cancellationToken);
    Task UpdateUserAsync(int id, AdminUpdateUserDto dto, CancellationToken cancellationToken);
    Task DeleteUserAsync(int id, CancellationToken cancellationToken);
}

