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

    public UserService(
        IUserRepository users,
        TokenService tokenService,
        IPasswordHasher<User> passwordHasher)
    {
        _users = users;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    // ===================== GET USERS =====================
    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(
        AdminUsersQueryDto query,
        CancellationToken cancellationToken)
    {
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var pageSize = query.PageSize switch
        {
            <= 0 => 10,
            > 100 => 100,
            _ => query.PageSize
        };

        var usersQuery = _users.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            usersQuery = usersQuery.Where(u =>
                u.Name.Contains(search) || u.Username.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim();
            usersQuery = usersQuery.Where(u => u.Role == role);
        }

        if (query.IsActive.HasValue)
            usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);

        if (query.IsApproved.HasValue)
            usersQuery = usersQuery.Where(u => u.IsApproved == query.IsApproved.Value);

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var entities = await usersQuery
            .OrderByDescending(u => u.AddedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminUserDto>
        {
            Items = entities.Select(AdminUserMapper.ToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // ===================== CREATE USER =====================
    public async Task<AdminUserDto> CreateUserAsync(
        AdminCreateUserDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ServiceException("Name and password are required.");
        }

        var role = (dto.Role ?? string.Empty).Trim();
        if (!Roles.All.Contains(role))
        {
            throw new ServiceException("Invalid role. Allowed: Child, Parent, Teacher, Admin.");
        }

        var username = (dto.Username ?? string.Empty).Trim();

        // Child username override
        if (string.Equals(role, Roles.Child, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(dto.RegistrationNo))
        {
            username = dto.RegistrationNo.Trim();
        }

        if (string.IsNullOrWhiteSpace(username))
            throw new ServiceException("Username is required.");

        // Username length validation (matches DB column varchar(50))
        if (username.Length > 50)
            throw new ServiceException("Username must not exceed 50 characters.");

        if (await _users.UsernameExistsAsync(username, null, cancellationToken))
            throw new ServiceException("Username already exists.");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Username = username,
            Role = role,
            IsApproved = dto.IsApproved,
            IsActive = dto.IsActive,
            AddedOn = DateTime.UtcNow,
            Password = dto.Password.Trim()   // stored as plain-text (hashing disabled)
        };

        try
        {
            await _users.AddAsync(user, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            throw new ServiceException($"Database insert failed: {message}");
        }

        return AdminUserMapper.ToDto(user);
    }

    // ===================== UPDATE USER =====================
    public async Task UpdateUserAsync(
        int id,
        AdminUpdateUserDto dto,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);

        if (user is null)
            throw new ServiceException("User not found.", StatusCodes.Status404NotFound);

        // ✅ Only update role if provided
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var role = dto.Role.Trim();

            if (!Roles.All.Contains(role))
                throw new ServiceException("Invalid role.");

            user.Role = role;
        }

        user.IsApproved = dto.IsApproved;
        user.IsActive = dto.IsActive;

        try
        {
            await _users.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            throw new ServiceException($"Database update failed: {message}");
        }
    }

    // ===================== DELETE USER (SOFT DELETE) =====================
    public async Task DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);

        if (user is null)
            throw new ServiceException("User not found.", StatusCodes.Status404NotFound);

        // Prevent deleting last admin
        if (string.Equals(user.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            var otherAdmins = await _users.Query().AsNoTracking()
                .CountAsync(u =>
                    u.Id != id &&
                    u.IsActive &&
                    u.Role == Roles.Admin,
                    cancellationToken);

            if (otherAdmins == 0)
                throw new ServiceException("Cannot delete the last active admin.");
        }

        user.IsActive = false;
        user.IsApproved = false;

        await _users.SaveChangesAsync(cancellationToken);
    }

    // ===================== LOGIN =====================
    public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken)
    {
        var username = (dto.Username ?? string.Empty).Trim();
        var password = (dto.Password ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new ServiceException("Username and password are required.");
        }

        var user = await _users.GetByUsernameAsync(username, cancellationToken);

        if (user is null)
            throw new ServiceException("Incorrect credentials");

        PasswordVerificationResult verifyResult;

        try
        {
            verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        }
        catch
        {
            verifyResult = PasswordVerificationResult.Failed;
        }

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            // fallback for plain-text passwords
            if (!string.Equals(user.Password?.Trim(), password, StringComparison.Ordinal))
                throw new ServiceException("Incorrect credentials");

            // plain-text match succeeded — no rehash
        }
        else if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // rehash skipped (plain-text mode)
        }

        if (!user.IsActive)
            throw new ServiceException("Account is disabled.");

        if (!user.IsApproved)
            throw new ServiceException("Account is pending approval.");

        var token = _tokenService.GenerateJWT(user);

        return new AuthResponseDto(
            new UserDto(user.Id, user.Name, false, user.Role),
            token);
    }
}