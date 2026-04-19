using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly TokenService _tokenService;
        public AccountController(DataContext dataContext, TokenService tokenService)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellationToken)
        {
            return BadRequest("Self-registration is disabled. Contact an administrator to create your account.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            var username = (dto.Username ?? string.Empty).Trim();
            var password = (dto.Password ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await _dataContext.Users
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

            if (user is null || !string.Equals(user.Password?.Trim(), password, StringComparison.Ordinal))
            {
                return BadRequest("Incorrect credentials");
            }

            if (!user.IsActive)
            {
                return BadRequest("Account is disabled. Contact an administrator.");
            }

            if (!user.IsApproved)
            {
                return BadRequest("Account is pending approval. Contact an administrator.");
            }

            var token = _tokenService.GenerateJWT(user);
            var response = new AuthResponseDto(new UserDto(user.Id, user.Name, false, user.Role), token);
            return Ok(response);
        }
    }
}
