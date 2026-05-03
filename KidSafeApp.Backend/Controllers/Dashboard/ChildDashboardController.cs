using KidSafeApp.Backend.Controllers.Common;
using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Domain.Auth;
using KidSafeApp.Shared.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Controllers.Dashboard;

[ApiController]
[Route("api/dashboard/child")]
[Authorize(Roles = Roles.Child)]
public sealed class ChildDashboardController : BaseController
{
    private static readonly string[] FlagKeywords =
    [
        "bully",
        "loser",
        "idiot",
        "stupid",
        "hate",
        "kill",
        "fight",
        "pathetic"
    ];

    private readonly DataContext _context;

    public ChildDashboardController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ChildDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == UserId && u.Role == Roles.Child, cancellationToken);

        if (user is null)
        {
            return NotFound("Child user not found.");
        }

        var classRoom = await _context.ClassRoomStudents
            .AsNoTracking()
            .Where(cs => cs.StudentId == UserId)
            .Select(cs => cs.ClassRoom)
            .FirstOrDefaultAsync(cancellationToken);

        var messages = await _context.Messages
            .AsNoTracking()
            .Where(m => m.FromId == UserId || m.ToId == UserId)
            .Select(m => m.Content)
            .ToListAsync(cancellationToken);

        var totalChats = messages.Count;
        var flaggedCount = messages.Count(IsFlaggedMessage);
        var safePercent = totalChats == 0
            ? 0
            : Math.Max(0, 100 - (int)Math.Round((flaggedCount * 100d) / totalChats));

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == UserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        var badges = notifications
            .Where(n => IsBadgeNotification(n.Title, n.Message, n.Type))
            .Take(4)
            .Select(n => new ChildBadgeDto
            {
                Title = n.Title,
                Subtitle = n.Message
            })
            .ToList();

        var badgeCount = notifications.Count(n => IsBadgeNotification(n.Title, n.Message, n.Type));

        var progressRows = await _context.UserProgress
            .AsNoTracking()
            .Where(p => p.UserId == UserId)
            .ToListAsync(cancellationToken);

        var totalPoints = progressRows.Sum(p => p.TotalPoints);
        var maxStreak = progressRows.Count == 0 ? 0 : progressRows.Max(p => p.Streak);
        var goal = Math.Max(100, ((totalPoints / 100) + 1) * 100);
        var remaining = Math.Max(0, goal - totalPoints);

        var tip = notifications.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n.Message));

        var dto = new ChildDashboardDto
        {
            DisplayName = user.Name,
            Greeting = GetGreeting(DateTime.UtcNow),
            ClassLabel = classRoom is null
                ? "No class assigned"
                : string.IsNullOrWhiteSpace(classRoom.Grade)
                    ? classRoom.Name
                    : $"Grade {classRoom.Grade}-{classRoom.Section}",
            SafeStreakDays = maxStreak,
            TotalChats = totalChats,
            SafeMessagePercent = safePercent,
            BadgesEarned = badgeCount,
            PointsEarned = totalPoints,
            PointsGoal = goal,
            PointsRemaining = remaining,
            TipTitle = tip?.Title,
            TipMessage = tip?.Message,
            RecentBadges = badges
        };

        return Ok(dto);
    }

    private static bool IsFlaggedMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        return FlagKeywords.Any(k => content.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBadgeNotification(string? title, string? message, string? type)
    {
        var joined = string.Join(" ", title, message, type);
        return joined.Contains("badge", StringComparison.OrdinalIgnoreCase)
            || joined.Contains("achievement", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetGreeting(DateTime utcNow)
    {
        var hour = utcNow.ToLocalTime().Hour;
        if (hour < 12) return "Good morning";
        if (hour < 18) return "Good afternoon";
        return "Good evening";
    }
}
