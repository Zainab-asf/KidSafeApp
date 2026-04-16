using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers
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
            var usernameExists = await _dataContext.Users
                                                    .AsNoTracking()
                                                    .AnyAsync(u => u.Username == dto.Username);
            if (usernameExists)
            {
                return BadRequest($"{nameof(dto.Username)} already exists");
            }

            var user = new User
            {
                Username = dto.Username,
                AddedOn = DateTime.Now,
                Name = dto.Name,
                Password = dto.Password, // Plain Password. Implement your own secure password mechanism
                Role = "Child",
                IsApproved = false,
                IsActive = true,
            };

            await _dataContext.Users.AddAsync(user, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);

            var token = _tokenService.GenerateJWT(user);
            var response = new AuthResponseDto(new UserDto(user.Id, user.Name, false), token);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == dto.Username && u.Password == dto.Password, cancellationToken);
            if (user is null)
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
            var response = new AuthResponseDto(new UserDto(user.Id, user.Name, false), token);
            return Ok(response);
        }
    }
}
