using KidSafe.Backend.DTOs;
using KidSafe.Backend.Services;
using Microsoft.AspNetCore.Mvc;

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
        if (result == null) return Conflict("Email already registered.");
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _auth.LoginAsync(dto);
        if (result == null) return Unauthorized("Invalid credentials.");
        return Ok(result);
    }
}
