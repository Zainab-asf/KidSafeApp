using KidSafeApp.Backend.Controllers.Common;
using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Domain.Auth;
using KidSafeApp.Shared.DTOs.Dashboard;
using KidSafeApp.Shared.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Controllers.Dashboard;

[ApiController]
[Route("api/dashboard/parent")]
[Authorize(Roles = Roles.Parent)]
public sealed class ParentDashboardController : BaseController
{
    private readonly DataContext _context;

    public ParentDashboardController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ParentDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == UserId && u.Role == Roles.Parent, cancellationToken);

        if (user is null)
        {
            return NotFound("Parent user not found.");
        }

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == UserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        var notificationDtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt,
            ExpiresAt = n.ExpiresAt
        }).ToList();

        var flaggedCount = notificationDtos.Count(IsFlaggedSignal);
        var unreadAlerts = notificationDtos.Count(n => !n.IsRead);
        var safeCount = notificationDtos.Count - flaggedCount;
        var safeRate = notificationDtos.Count == 0
            ? 0
            : (int)Math.Round((safeCount * 100d) / notificationDtos.Count);

        var badgesEarned = notificationDtos.Count(n => IsBadgeNotification(n.Title, n.Message, n.Type));
        var newFlags = notificationDtos.Count(n => !n.IsRead && IsFlaggedSignal(n));

        var recentAlerts = notificationDtos.Take(4).ToList();

        var weekly = BuildWeekly(notificationDtos);

        var childLabel = "";
        var childGrade = "";
        var hasLinkedChild = false;

        var dto = new ParentDashboardDto
        {
            ParentName = user.Name,
            ChildName = childLabel,
            ChildGrade = childGrade,
            HasLinkedChild = hasLinkedChild,
            SafeRate = safeRate,
            FlaggedCount = flaggedCount,
            UnreadAlerts = unreadAlerts,
            BadgesEarned = badgesEarned,
            SafeMessages = safeCount,
            SafeBar = safeRate,
            FlaggedBar = notificationDtos.Count == 0 ? 0 : 100 - safeRate,
            BadgeBar = notificationDtos.Count == 0 ? 0 : (int)Math.Round((badgesEarned * 100d) / notificationDtos.Count),
            SafeStreak = GetSafeStreakDays(notificationDtos),
            NewFlags = newFlags,
            Weekly = weekly,
            RecentAlerts = recentAlerts
        };

        return Ok(dto);
    }

    private static int GetSafeStreakDays(IEnumerable<NotificationDto> notifications)
    {
        var lastFlagged = notifications
            .Where(IsFlaggedSignal)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefault();

        if (lastFlagged is null)
        {
            return 0;
        }

        var days = (DateTime.UtcNow - lastFlagged.CreatedAt).Days;
        return Math.Max(0, days);
    }

    private static List<WeeklyPointDto> BuildWeekly(IEnumerable<NotificationDto> notifications)
    {
        var today = DateTime.UtcNow.Date;
        var points = new List<WeeklyPointDto>();

        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var items = notifications.Where(n => n.CreatedAt.Date == day).ToList();
            var total = items.Count;
            var blocked = items.Count(IsBlockedSignal);
            var flagged = items.Count(IsFlaggedSignal) - blocked;
            var safe = total - blocked - flagged;

            points.Add(new WeeklyPointDto
            {
                Day = day.ToString("ddd"),
                Safe = total == 0 ? 0 : (int)Math.Round((safe * 100d) / total),
                Flagged = total == 0 ? 0 : (int)Math.Round((flagged * 100d) / total),
                Blocked = total == 0 ? 0 : (int)Math.Round((blocked * 100d) / total)
            });
        }

        return points;
    }

    private static bool IsFlaggedSignal(NotificationDto notification)
    {
        var title = notification.Title ?? string.Empty;
        var message = notification.Message ?? string.Empty;
        var type = notification.Type ?? string.Empty;

        return type.Contains("warn", StringComparison.OrdinalIgnoreCase)
            || type.Contains("alert", StringComparison.OrdinalIgnoreCase)
            || title.Contains("flag", StringComparison.OrdinalIgnoreCase)
            || title.Contains("block", StringComparison.OrdinalIgnoreCase)
            || message.Contains("flag", StringComparison.OrdinalIgnoreCase)
            || message.Contains("block", StringComparison.OrdinalIgnoreCase)
            || message.Contains("inappropriate", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBlockedSignal(NotificationDto notification)
    {
        var title = notification.Title ?? string.Empty;
        var message = notification.Message ?? string.Empty;
        var type = notification.Type ?? string.Empty;

        return type.Contains("block", StringComparison.OrdinalIgnoreCase)
            || title.Contains("block", StringComparison.OrdinalIgnoreCase)
            || message.Contains("block", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBadgeNotification(string? title, string? message, string? type)
    {
        var joined = string.Join(" ", title, message, type);
        return joined.Contains("badge", StringComparison.OrdinalIgnoreCase)
            || joined.Contains("achievement", StringComparison.OrdinalIgnoreCase);
    }
}