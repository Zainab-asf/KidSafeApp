using KidSafeApp.Backend.Data;
using KidSafeApp.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public sealed class AdminController : ControllerBase
{
    private readonly DataContext _dataContext;

    public AdminController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _dataContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.AddedOn)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Name,
                u.Username,
                u.Role,
                u.IsApproved,
                u.IsActive,
                u.AddedOn
            ))
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var role = (dto.Role ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("Role is required.");
        }

        var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Child", "Parent", "Teacher", "Admin" };
        if (!allowedRoles.Contains(role))
        {
            return BadRequest("Invalid role. Allowed: Child, Parent, Teacher, Admin.");
        }

        user.Role = role;
        user.IsApproved = dto.IsApproved;
        user.IsActive = dto.IsActive;

        await _dataContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("bootstrap")]
    [AllowAnonymous]
    public async Task<IActionResult> BootstrapAdmin([FromQuery] string key, CancellationToken cancellationToken)
    {
        // One-time helper endpoint for school/dev to create the first admin.
        // Configure the key via appsettings: Admin:BootstrapKey
        var expected = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Admin:BootstrapKey"];
        if (string.IsNullOrWhiteSpace(expected) || !string.Equals(expected, key, StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var anyAdmin = await _dataContext.Users.AsNoTracking().AnyAsync(u => u.Role == "Admin", cancellationToken);
        if (anyAdmin)
        {
            return BadRequest("Admin already exists.");
        }

        var adminUser = new KidSafeApp.Backend.Data.Entities.User
        {
            Name = "School Admin",
            Username = "admin",
            Password = "admin",
            AddedOn = DateTime.Now,
            Role = "Admin",
            IsApproved = true,
            IsActive = true
        };

        await _dataContext.Users.AddAsync(adminUser, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);

        return Ok(new { adminUser.Username, adminUser.Password });
    }
}
