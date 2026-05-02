using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Backend.Domain.Auth;
using KidSafeApp.Shared.DTOs.Admin;
using KidSafeApp.Shared.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Services.Users;

public sealed class UserService : IUserService
{
    private readonly DataContext _db;

    public UserService(DataContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(AdminUsersQueryDto query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 50 : query.PageSize, 1, 500);

        IQueryable<User> users = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            users = users.Where(u => u.Name.Contains(s) || u.Username.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim();
            users = users.Where(u => u.Role == role);
        }

        if (query.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == query.IsActive.Value);
        }

        if (query.IsApproved.HasValue)
        {
            users = users.Where(u => u.IsApproved == query.IsApproved.Value);
        }

        var total = await users.CountAsync(cancellationToken);

        var items = await users
            .OrderByDescending(u => u.AddedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto(u.Id, u.Name, u.Username, u.Role, u.IsApproved, u.IsActive, u.AddedOn))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminUserDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserDto> CreateUserAsync(AdminCreateUserDto dto, CancellationToken cancellationToken)
    {
        var role = string.IsNullOrWhiteSpace(dto.Role) ? Roles.Child : dto.Role.Trim();
        if (role is not (Roles.Admin or Roles.Child or Roles.Parent or Roles.Teacher))
        {
            throw new ServiceException("Invalid role.", StatusCodes.Status400BadRequest);
        }

        var username = (dto.Username ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ServiceException("Username is required.", StatusCodes.Status400BadRequest);
        }

        var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Username == username, cancellationToken);
        if (exists)
        {
            throw new ServiceException("Username already exists.", StatusCodes.Status400BadRequest);
        }

        var user = new User
        {
            Name = (dto.Name ?? string.Empty).Trim(),
            Username = username,
            Password = (dto.Password ?? string.Empty).Trim(),
            Role = role,
            IsApproved = dto.IsApproved,
            IsActive = dto.IsActive,
            AddedOn = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return new AdminUserDto(user.Id, user.Name, user.Username, user.Role, user.IsApproved, user.IsActive, user.AddedOn);
    }

    public async Task UpdateUserAsync(int id, AdminUpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            throw new ServiceException("User not found.", StatusCodes.Status404NotFound);
        }

        user.Role = string.IsNullOrWhiteSpace(dto.Role) ? user.Role : dto.Role.Trim();
        user.IsActive = dto.IsActive;
        user.IsApproved = dto.IsApproved;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

