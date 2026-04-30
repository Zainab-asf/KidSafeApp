using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Backend.Domain.Admin;
using KidSafeApp.Backend.Domain.Auth;
using KidSafeApp.Backend.Repositories.Users;
using KidSafeApp.Shared.DTOs.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Services.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly TokenService _tokenService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository users, TokenService tokenService, IPasswordHasher<User> passwordHasher)
    {
        _users = users;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(AdminUsersQueryDto query, CancellationToken cancellationToken)
    {
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var pageSize = query.PageSize switch { <= 0 => 10, > 100 => 100, _ => query.PageSize };

        var usersQuery = _users.Query().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            usersQuery = usersQuery.Where(u => u.Name.Contains(search) || u.Username.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim();
            usersQuery = usersQuery.Where(u => u.Role == role);
        }

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
        }

        if (query.IsApproved.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.IsApproved == query.IsApproved.Value);
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var entities = await usersQuery
            .OrderByDescending(u => u.AddedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = entities.Select(AdminUserMapper.ToDto).ToList();

        return new PagedResultDto<AdminUserDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AdminUserDto> CreateUserAsync(AdminCreateUserDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ServiceException("Name, username and password are required.");
        }

        var role = (dto.Role ?? string.Empty).Trim();
        if (!Roles.All.Contains(role))
        {
            throw new ServiceException("Invalid role. Allowed: Child, Parent, Teacher, Admin.");
        }

        var username = (dto.Username ?? string.Empty).Trim();
        if (string.Equals(role, Roles.Child, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(dto.RegistrationNo))
        {
            username = dto.RegistrationNo.Trim();
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ServiceException("Username is required.");
        }

        var exists = await _users.UsernameExistsAsync(username, null, cancellationToken);
        if (exists)
        {
            throw new ServiceException("Username already exists.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Username = username,
            Role = role,
            IsApproved = dto.IsApproved,
            IsActive = dto.IsActive,
            AddedOn = DateTime.UtcNow
        };
        user.Password = _passwordHasher.HashPassword(user, dto.Password.Trim());

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);
        return AdminUserMapper.ToDto(user);
    }

    public async Task UpdateUserAsync(int id, AdminUpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new ServiceException("User not found.", StatusCodes.Status404NotFound);
        }

        var role = (dto.Role ?? string.Empty).Trim();
        if (!Roles.All.Contains(role))
        {
            throw new ServiceException("Invalid role. Allowed: Child, Parent, Teacher, Admin.");
        }

        user.Role = role;
        user.IsApproved = dto.IsApproved;
        user.IsActive = dto.IsActive;
        await _users.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new ServiceException("User not found.", StatusCodes.Status404NotFound);
        }

        if (string.Equals(user.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            var otherActiveAdmins = await _users.Query().AsNoTracking()
                .CountAsync(u => u.Id != id && u.IsActive && u.Role == Roles.Admin, cancellationToken);
            if (otherActiveAdmins == 0)
            {
                throw new ServiceException("Cannot delete the last active admin.");
            }
        }

        user.IsActive = false;
        user.IsApproved = false;
        await _users.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var username = (dto.Username ?? string.Empty).Trim();
        var password = (dto.Password ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new ServiceException("Username and password are required.");
        }

        var user = await _users.GetByUsernameAsync(username, cancellationToken);
        if (user is null)
        {
            throw new ServiceException("Incorrect credentials");
        }

        PasswordVerificationResult verifyResult;
        try
        {
            verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        }
        catch (FormatException)
        {
            verifyResult = PasswordVerificationResult.Failed;
        }

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            // Backward-compatible fallback for existing clear-text rows.
            if (!string.Equals(user.Password?.Trim(), password, StringComparison.Ordinal))
            {
                throw new ServiceException("Incorrect credentials");
            }

            user.Password = _passwordHasher.HashPassword(user, password);
        }
        else if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = _passwordHasher.HashPassword(user, password);
            await _users.SaveChangesAsync(cancellationToken);
        }

        if (!user.IsActive)
        {
            throw new ServiceException("Account is disabled. Contact an administrator.");
        }

        if (!user.IsApproved)
        {
            throw new ServiceException("Account is pending approval. Contact an administrator.");
        }

        var token = _tokenService.GenerateJWT(user);
        return new AuthResponseDto(new UserDto(user.Id, user.Name, false, user.Role), token);
    }
}
