using System.Security.Claims;
using KidSafe.Backend.Data;
using KidSafe.Backend.Data.Entities;
using KidSafe.Backend.DTOs;
using KidSafe.Backend.Hubs;
using KidSafe.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KidSafe.Backend.Controllers;

/// <summary>
/// AI label mapping (SDD §4.1 / SRS FR-CB-01):
///   Safe  → deliver + award 10 pts
///   Watch → mask + store + alert  (was "flagged")
///   Review → block + store + alert (was "blocked")
/// </summary>
[ApiController]
[Route("messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAIService _ai;
    private readonly INotificationService _notifications;
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public MessagesController(AppDbContext db, IAIService ai,
        INotificationService notifications, IHubContext<ChatHub, IChatClient> hub)
    {
        _db            = db;
        _ai            = ai;
        _notifications = notifications;
        _hub           = hub;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(SendMessageDto dto)
    {
        var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var sender   = await _db.Users.FindAsync(senderId);
        if (sender == null) return Unauthorized();

        var analysis = await _ai.AnalyzeAsync(dto.Message);
        var label    = MapLabel(analysis.Label);  // normalise AI output to Safe/Watch/Review
        var masked   = _ai.MaskMessage(dto.Message);

        if (label == "Safe")
        {
            // Award points (SRS FR-AP-01)
            var reward = await _db.Rewards.FirstOrDefaultAsync(r => r.UserId == senderId);
            if (reward != null)
            {
                reward.Points += 10;
                await _db.SaveChangesAsync();
                await AwardBadgeIfEligible(reward);
            }

            await _hub.Clients.Group($"user_{dto.ReceiverId}")
                .ReceiveMessage(senderId, sender.DisplayName, dto.Message, "Safe");

            return Ok(new MessageResultDto("sent", null, "Safe", analysis.Score));
        }

        // Watch or Review — store in FlaggedMessages (SRS FR-CB-01)
        var flagged = new FlaggedMessage
        {
            SenderId      = senderId,
            ReceiverId    = dto.ReceiverId,
            Message       = dto.Message,
            MaskedMessage = masked,
            Label         = label,
            Score         = analysis.Score
        };
        _db.FlaggedMessages.Add(flagged);
        await _db.SaveChangesAsync();

        // Push to all parents + teachers (SRS FR-NO-01)
        var parentTokens = await _db.Users
            .Where(u => (u.Role == "Parent" || u.Role == "Teacher") && u.FcmToken != null)
            .Select(u => u.FcmToken!)
            .ToListAsync();

        await _notifications.BroadcastToParentsAsync(
            sender.DisplayName, label, masked, parentTokens);

        // Real-time dashboard alert
        await _hub.Clients.Group("parents")
            .FlaggedMessageAlert(senderId, sender.DisplayName, masked, label, analysis.Score);

        if (label == "Watch")
        {
            // Deliver masked version to receiver
            await _hub.Clients.Group($"user_{dto.ReceiverId}")
                .ReceiveMessage(senderId, sender.DisplayName, masked, "Watch");
            return Ok(new MessageResultDto("masked", masked, "Watch", analysis.Score));
        }

        // Review — blocked, not delivered
        return Ok(new MessageResultDto("blocked", null, "Review", analysis.Score));
    }

    [HttpGet("flagged")]
    [Authorize(Roles = "Parent,Teacher,Admin")]
    public async Task<IActionResult> GetFlagged()
    {
        var messages = await _db.FlaggedMessages
            .Include(f => f.Sender)
            .OrderByDescending(f => f.Timestamp)
            .Select(f => new
            {
                f.Id,
                SenderName    = f.Sender.DisplayName,
                f.SenderId,
                f.ReceiverId,
                f.MaskedMessage,
                f.Label,
                f.Score,
                f.Timestamp
            })
            .ToListAsync();

        return Ok(messages);
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    /// <summary>Normalise AI output → Safe | Watch | Review (SDD §4.1)</summary>
    private static string MapLabel(string raw) => raw.ToLowerInvariant() switch
    {
        "safe"    => "Safe",
        "watch"   => "Watch",
        "flagged" => "Watch",
        "review"  => "Review",
        "blocked" => "Review",
        _         => "Watch"
    };

    private async Task AwardBadgeIfEligible(Reward reward)
    {
        var badges = System.Text.Json.JsonSerializer
            .Deserialize<List<string>>(reward.Badges) ?? new List<string>();

        string? newBadge = reward.Points switch
        {
            >= 10000 when !badges.Contains("Legend")        => "Legend",
            >= 5000  when !badges.Contains("Safety King")   => "Safety King",
            >= 2000  when !badges.Contains("Chat Scholar")  => "Chat Scholar",
            >= 1000  when !badges.Contains("Cyber Hero")    => "Cyber Hero",
            >= 500   when !badges.Contains("Kind Star")     => "Kind Star",
            >= 100   when !badges.Contains("Safe Chatter")  => "Safe Chatter",
            _ => null
        };

        if (newBadge != null)
        {
            badges.Add(newBadge);
            reward.Badges = System.Text.Json.JsonSerializer.Serialize(badges);
            await _db.SaveChangesAsync();
        }
    }
}
