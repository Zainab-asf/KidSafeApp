using KidSafe.Backend.Common;
using KidSafe.Backend.Data.Entities;
using KidSafe.Backend.DTOs;

namespace KidSafe.Backend.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    string GenerateJwt(User user);
}
