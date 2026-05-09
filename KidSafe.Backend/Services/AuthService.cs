using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KidSafe.Backend.Common;
using KidSafe.Backend.Data;
using KidSafe.Backend.Data.Entities;
using KidSafe.Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KidSafe.Backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext    _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Result<AuthResponseDto>.Conflict("Email already registered.");

        // Admin can only be created via seeding
        var validRoles = new[] { AppConstants.Roles.Child, AppConstants.Roles.Parent, AppConstants.Roles.Teacher };
        var role       = validRoles.Contains(dto.Role) ? dto.Role : AppConstants.Roles.Child;

        // Teachers start pending until Admin approves
        var status = role == AppConstants.Roles.Teacher
            ? AppConstants.UserStatus.Pending
            : AppConstants.UserStatus.Active;

        var user = new User
        {
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DisplayName  = dto.DisplayName,
            Role         = role,
            Status       = status
        };

        _db.Users.Add(user);
        _db.Rewards.Add(new Reward { User = user });
        await _db.SaveChangesAsync();

        var token = status == AppConstants.UserStatus.Pending
            ? AppConstants.UserStatus.Pending
            : GenerateJwt(user);

        return Result<AuthResponseDto>.Ok(
            new AuthResponseDto(token, user.Role, user.Id, user.DisplayName, user.AvatarEmoji, user.Email));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Fail("Invalid credentials.", 401);

        var token = user.Status switch
        {
            AppConstants.UserStatus.Pending  => AppConstants.UserStatus.Pending,
            AppConstants.UserStatus.Disabled => AppConstants.UserStatus.Disabled,
            _                                => GenerateJwt(user)
        };

        return Result<AuthResponseDto>.Ok(
            new AuthResponseDto(token, user.Role, user.Id, user.DisplayName, user.AvatarEmoji, user.Email));
    }

    public string GenerateJwt(User user)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim("displayName",             user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
