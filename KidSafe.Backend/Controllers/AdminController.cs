using System.Security.Claims;
using KidSafe.Backend.Data;
using KidSafe.Backend.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafe.Backend.Controllers;

/// <summary>
/// Admin operations:
///   GET  /admin/users                    – list all users
///   GET  /admin/users/pending            – pending teacher accounts
///   POST /admin/users                    – create any role user
///   POST /admin/users/{id}/approve       – approve teacher account
///   POST /admin/users/{id}/disable       – disable a user
///   GET  /admin/moderation               – moderation history
///   POST /admin/moderation               – apply moderation action
///   GET  /admin/parent-links             – all parent-child links
///   POST /admin/parent-links             – link parent ↔ child
///   DELETE /admin/parent-links/{pid}/{cid} – unlink
/// </summary>
[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // ── Create User (Admin-only registration) ──────────────────────

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Email already in use.");

        var user = new User
        {
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DisplayName  = dto.DisplayName,
            Role         = dto.Role,
            Status       = "active",
            AvatarEmoji  = dto.AvatarEmoji
        };
        _db.Users.Add(user);
        _db.Rewards.Add(new Reward { User = user });
        await _db.SaveChangesAsync();

        // Enroll student in class immediately if ClassId provided
        if (dto.ClassId.HasValue && dto.Role == "Child")
        {
            var cls = await _db.Classes.FindAsync(dto.ClassId.Value);
            if (cls != null && !await _db.ClassStudents.AnyAsync(
                    cs => cs.ClassId == dto.ClassId.Value && cs.StudentId == user.Id))
            {
                _db.ClassStudents.Add(new ClassStudent { ClassId = dto.ClassId.Value, StudentId = user.Id });
                await _db.SaveChangesAsync();
            }
        }

        return Ok(new { user.Id, user.Email, user.DisplayName, user.Role });
    }

    // ── UC-11 Manage Users ──────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .Select(u => new { u.Id, u.Email, u.DisplayName, u.Role, u.Status, u.CreatedAt })
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("users/pending")]
    public async Task<IActionResult> GetPendingTeachers()
    {
        var pending = await _db.Users
            .Where(u => u.Status == "pending")
            .Select(u => new { u.Id, u.Email, u.DisplayName, u.Role, u.CreatedAt })
            .ToListAsync();
        return Ok(pending);
    }

    // ── UC-12 Approve Account ───────────────────────────────────────

    [HttpPost("users/{id:int}/approve")]
    public async Task<IActionResult> ApproveUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Status = "active";
        await _db.SaveChangesAsync();
        return Ok(new { message = $"{user.DisplayName} approved." });
    }

    [HttpPost("users/{id:int}/disable")]
    public async Task<IActionResult> DisableUser(int id)
    {
        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (id == adminId) return BadRequest("Cannot disable your own account.");
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Status = "disabled";
        await _db.SaveChangesAsync();
        return Ok(new { message = $"{user.DisplayName} disabled." });
    }

    // ── UC-09 Apply Moderation Action ───────────────────────────────

    [HttpPost("moderation")]
    public async Task<IActionResult> ApplyAction([FromBody] ApplyModerationDto dto)
    {
        var actorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var target  = await _db.Users.FindAsync(dto.TargetUserId);
        if (target == null) return NotFound("Target user not found.");

        var action = new ModerationAction
        {
            Type             = dto.Type,
            ActorId          = actorId,
            TargetUserId     = dto.TargetUserId,
            FlaggedMessageId = dto.FlaggedMessageId,
            Notes            = dto.Notes ?? string.Empty
        };
        _db.ModerationActions.Add(action);

        // Apply status change for block/mute
        if (dto.Type == "block")
            target.Status = "disabled";

        await _db.SaveChangesAsync();
        return Ok(new { action.Id, action.Type, action.CreatedAt });
    }
    

    // ── UC-10 View Moderation History ───────────────────────────────

    [HttpGet("moderation")]
    public async Task<IActionResult> GetModerationHistory()
    {
        var history = await _db.ModerationActions
            .Include(ma => ma.Actor)
            .Include(ma => ma.TargetUser)
            .OrderByDescending(ma => ma.CreatedAt)
            .Select(ma => new
            {
                ma.Id,
                ma.Type,
                Actor      = ma.Actor.DisplayName,
                Target     = ma.TargetUser.DisplayName,
                ma.Notes,
                ma.CreatedAt,
                ma.RevokedAt
            })
            .ToListAsync();
        return Ok(history);
    }


    // ── Parent-Child Links ──────────────────────────────────────────

    [HttpGet("parent-links")]
    public async Task<IActionResult> GetParentLinks()
    {
        var links = await _db.ParentChildren
            .Include(pc => pc.Parent)
            .Include(pc => pc.Child)
            .Select(pc => new
            {
                pc.ParentId, ParentName = pc.Parent.DisplayName, ParentEmail = pc.Parent.Email,
                pc.ChildId,  ChildName  = pc.Child.DisplayName,  ChildEmail  = pc.Child.Email,
                pc.LinkedAt
            })
            .ToListAsync();
        return Ok(links);
    }

    [HttpPost("parent-links")]
    public async Task<IActionResult> LinkParentChild([FromBody] ParentLinkDto dto)
    {
        if (await _db.ParentChildren.AnyAsync(pc => pc.ParentId == dto.ParentId && pc.ChildId == dto.ChildId))
            return Conflict("Already linked.");

        var parent = await _db.Users.FindAsync(dto.ParentId);
        var child  = await _db.Users.FindAsync(dto.ChildId);
        if (parent == null || child == null) return NotFound("User not found.");
        if (parent.Role != "Parent") return BadRequest("First user must be a Parent.");
        if (child.Role  != "Child")  return BadRequest("Second user must be a Child.");

        _db.ParentChildren.Add(new ParentChild { ParentId = dto.ParentId, ChildId = dto.ChildId });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Linked." });
    }

    [HttpDelete("parent-links/{parentId:int}/{childId:int}")]
    public async Task<IActionResult> UnlinkParentChild(int parentId, int childId)
    {
        var link = await _db.ParentChildren.FindAsync(parentId, childId);
        if (link == null) return NotFound();
        _db.ParentChildren.Remove(link);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record ApplyModerationDto(string Type, int TargetUserId, int? FlaggedMessageId, string? Notes);
public record CreateUserDto(string Email, string Password, string DisplayName, string Role, string? AvatarEmoji, int? ClassId = null);
public record ParentLinkDto(int ParentId, int ChildId);
