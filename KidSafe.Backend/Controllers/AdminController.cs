using System.Security.Claims;
using KidSafe.Backend.Data;
using KidSafe.Backend.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafe.Backend.Controllers;

/// <summary>
/// Admin operations (SDD UC-11, UC-12, UC-09, UC-10):
///   GET  /admin/users                   – list all users
///   GET  /admin/users/pending            – pending teacher accounts
///   POST /admin/users/{id}/approve       – approve teacher account
///   POST /admin/users/{id}/disable       – disable a user
///   GET  /admin/moderation               – moderation history
///   POST /admin/moderation               – apply moderation action
/// </summary>
[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db) => _db = db;

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
}

public record ApplyModerationDto(string Type, int TargetUserId, int? FlaggedMessageId, string? Notes);
