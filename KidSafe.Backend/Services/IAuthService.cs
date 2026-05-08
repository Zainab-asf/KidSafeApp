using KidSafe.Backend.Data.Entities;
using KidSafe.Backend.DTOs;

namespace KidSafe.Backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    string GenerateJwt(User user);
}
