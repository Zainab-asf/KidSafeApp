using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KidSafe.Backend.Data;
using KidSafe.Backend.Data.Entities;
using KidSafe.Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KidSafe.Backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return null;

        // Admin can only be created via direct DB seeding (not via public register)
        var validRoles = new[] { "Child", "Parent", "Teacher" };
        var role = validRoles.Contains(dto.Role) ? dto.Role : "Child";

        // Teachers start as pending until Admin approves (SDD UC-12)
        var status = role == "Teacher" ? "pending" : "active";

        var user = new User
        {
            Email       = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DisplayName  = dto.DisplayName,
            Role         = role,
            Status       = status
        };

        _db.Users.Add(user);
        _db.Rewards.Add(new Reward { User = user });
        await _db.SaveChangesAsync();

        // Return token only for non-pending accounts; pending Teachers get null token
        if (status == "pending")
            return new AuthResponseDto("pending", user.Role, user.Id, user.DisplayName);

        return new AuthResponseDto(GenerateJwt(user), user.Role, user.Id, user.DisplayName);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        if (user.Status == "pending")
            return new AuthResponseDto("pending", user.Role, user.Id, user.DisplayName);

        if (user.Status == "disabled")
            return new AuthResponseDto("disabled", user.Role, user.Id, user.DisplayName);

        return new AuthResponseDto(GenerateJwt(user), user.Role, user.Id, user.DisplayName);
    }

    public string GenerateJwt(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("displayName", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"],
            audience:          _config["Jwt:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
