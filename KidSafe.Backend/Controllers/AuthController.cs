using KidSafe.Backend.Data;
using KidSafe.Backend.DTOs;
using KidSafe.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KidSafe.Backend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _auth.RegisterAsync(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _auth.LoginAsync(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPatch("avatar")]
    [Authorize]
    public async Task<IActionResult> UpdateAvatar(
        [FromBody] AvatarDto dto,
        [FromServices] AppDbContext db)
    {
        var uid  = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await db.Users.FindAsync(uid);
        if (user == null) return NotFound();
        user.AvatarEmoji = dto.Emoji;
        await db.SaveChangesAsync();
        return Ok();
    }
}

public record AvatarDto(string Emoji);
