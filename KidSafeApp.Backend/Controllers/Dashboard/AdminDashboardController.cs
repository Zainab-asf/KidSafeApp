using KidSafeApp.Backend.Controllers.Common;
using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Domain.Auth;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Shared.DTOs.Admin;
using KidSafeApp.Shared.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Controllers.Dashboard;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminDashboardController : BaseController
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

    public AdminDashboardController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AdminDashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var students = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == Roles.Child && u.IsActive)
            .ToListAsync(cancellationToken);

        var teachers = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == Roles.Teacher && u.IsActive)
            .ToListAsync(cancellationToken);

        var classRooms = await _context.ClassRooms
            .AsNoTracking()
            .Include(c => c.Students)
            .ToListAsync(cancellationToken);

        var messageRows = await _context.Messages
            .AsNoTracking()
            .Where(m => m.SentOn >= DateTime.UtcNow.AddDays(-7))
            .Select(m => new MessageRow(m.Content, m.SentOn))
            .ToListAsync(cancellationToken);

        var totalMessages = messageRows.Count;
        var flaggedMessages = messageRows.Count(m => IsFlaggedMessage(m.Content));

        var weekly = BuildWeekly(messageRows);

        var recentAlerts = await BuildRecentAlertsAsync(cancellationToken);

        var topTeachers = teachers
            .Select(t => new AdminTeacherStatDto
            {
                TeacherId = t.Id,
                Name = t.Name,
                Username = t.Username,
                ClassCount = classRooms.Count(c => c.TeacherId == t.Id)
            })
            .OrderByDescending(t => t.ClassCount)
            .ThenBy(t => t.Name)
            .Take(3)
            .ToList();

        var safetyScore = totalMessages == 0
            ? 100
            : Math.Max(0, 100 - (int)Math.Round((flaggedMessages * 100d) / totalMessages));

        var dto = new AdminDashboardSummaryDto
        {
            ActiveStudents = students.Count,
            ChatsMonitored = totalMessages,
            FlaggedMessages = flaggedMessages,
            BlockedUsers = await _context.Users.AsNoTracking().CountAsync(u => !u.IsActive, cancellationToken),
            SafetyScore = safetyScore,
            Weekly = weekly,
            RecentAlerts = recentAlerts,
            TopTeachers = topTeachers
        };

        return Ok(dto);
    }

    [HttpGet("child-chat")]
    public async Task<ActionResult<AdminChildChatOverviewDto>> GetChildChatOverview(CancellationToken cancellationToken)
    {
        var children = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == Roles.Child && u.IsActive)
            .ToListAsync(cancellationToken);

        var childIds = children.Select(c => c.Id).ToList();

        var classRoomLabels = await _context.ClassRoomStudents
            .AsNoTracking()
            .Where(cs => childIds.Contains(cs.StudentId))
            .Select(cs => new { cs.StudentId, cs.ClassRoom.Name, cs.ClassRoom.Grade, cs.ClassRoom.Section })
            .ToListAsync(cancellationToken);

        var labelMap = classRoomLabels
            .GroupBy(x => x.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => string.IsNullOrWhiteSpace(x.Grade)
                    ? x.Name
                    : $"Grade {x.Grade}-{x.Section}").FirstOrDefault() ?? string.Empty);

        var messageRows = await _context.Messages
            .AsNoTracking()
            .Where(m => childIds.Contains(m.FromId) || childIds.Contains(m.ToId))
            .Select(m => new MessageRowWithUsers(m.FromId, m.ToId, m.Content, m.SentOn))
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var riskMap = new Dictionary<int, string>();

        foreach (var childId in childIds)
        {
            var recentMessages = messageRows
                .Where(m => m.FromId == childId || m.ToId == childId)
                .Where(m => m.SentOn >= now.AddDays(-7))
                .ToList();

            var flagged = recentMessages.Count(m => IsFlaggedMessage(m.Content));

            riskMap[childId] = flagged >= 3
                ? "Blocked"
                : flagged >= 1
                    ? "Warning"
                    : "Safe";
        }

        var students = children.Select(child =>
        {
            var childMessages = messageRows
                .Where(m => m.FromId == child.Id || m.ToId == child.Id)
                .OrderByDescending(m => m.SentOn)
                .ToList();

            var lastMessage = childMessages.FirstOrDefault();

            return new AdminChildChatStudentDto
            {
                Id = child.Id,
                Name = child.Name,
                Username = child.Username,
                GradeLabel = labelMap.TryGetValue(child.Id, out var label) ? label : string.Empty,
                RiskLevel = riskMap.TryGetValue(child.Id, out var risk) ? risk : "Safe",
                LastMessageAt = lastMessage?.SentOn,
                UnreadCount = childMessages.Count(m => m.SentOn >= now.AddDays(-1)),
                LastMessagePreview = lastMessage?.Content
            };
        }).ToList();

        var dto = new AdminChildChatOverviewDto
        {
            TotalStudents = children.Count,
            ActiveChats = students.Count(s => s.LastMessageAt.HasValue && s.LastMessageAt.Value >= now.AddDays(-7)),
            FlaggedToday = messageRows.Count(m => m.SentOn >= now.AddDays(-1) && IsFlaggedMessage(m.Content)),
            Blocked = riskMap.Values.Count(v => string.Equals(v, "Blocked", StringComparison.OrdinalIgnoreCase)),
            Students = students
        };

        return Ok(dto);
    }

    [HttpGet("child-chat/{studentId:int}/messages")]
    public async Task<ActionResult<List<AdminChatLineDto>>> GetChildChatMessages(int studentId, CancellationToken cancellationToken)
    {
        var child = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == Roles.Child, cancellationToken);

        if (child is null)
        {
            return NotFound("Child user not found.");
        }

        var messageRows = await _context.Messages
            .AsNoTracking()
            .Where(m => m.FromId == studentId || m.ToId == studentId)
            .OrderByDescending(m => m.SentOn)
            .Take(60)
            .Select(m => new MessageRowWithFrom(m.FromId, m.Content, m.SentOn))
            .ToListAsync(cancellationToken);

        var userNames = await _context.Users
            .AsNoTracking()
            .Where(u => messageRows.Select(x => x.FromId).Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        var dto = messageRows
            .OrderBy(m => m.SentOn)
            .Select(m => new AdminChatLineDto
            {
                FromUserId = m.FromId,
                FromName = userNames.TryGetValue(m.FromId, out var name) ? name : "User",
                Text = m.Content,
                SentAt = m.SentOn,
                IsAlert = IsFlaggedMessage(m.Content)
            })
            .ToList();

        return Ok(dto);
    }

    [HttpGet("parent-portal")]
    public async Task<ActionResult<AdminParentPortalSummaryDto>> GetParentPortal(CancellationToken cancellationToken)
    {
        var parents = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == Roles.Parent && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);

        var parentIds = parents.Select(p => p.Id).ToList();

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => parentIds.Contains(n.UserId))
            .ToListAsync(cancellationToken);

        var rows = parents.Select(p =>
        {
            var parentNotifications = notifications.Where(n => n.UserId == p.Id).ToList();
            var lastActive = parentNotifications.OrderByDescending(n => n.CreatedAt).FirstOrDefault()?.CreatedAt;

            return new AdminParentPortalRowDto
            {
                ParentId = p.Id,
                Name = p.Name,
                ChildName = string.Empty,
                ChildGrade = string.Empty,
                Email = p.Username,
                Phone = string.Empty,
                LastActiveAt = lastActive,
                PendingAlerts = parentNotifications.Count(IsFlaggedNotification)
            };
        }).ToList();

        var activeThisWeek = rows.Count(r => r.LastActiveAt.HasValue && r.LastActiveAt.Value >= DateTime.UtcNow.AddDays(-7));

        var dto = new AdminParentPortalSummaryDto
        {
            TotalParents = parents.Count,
            ActiveThisWeek = activeThisWeek,
            PendingAlerts = rows.Sum(r => r.PendingAlerts),
            Parents = rows
        };

        return Ok(dto);
    }

    [HttpGet("teacher-module")]
    public async Task<ActionResult<AdminTeacherModuleSummaryDto>> GetTeacherModule(CancellationToken cancellationToken)
    {
        var teachers = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == Roles.Teacher)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);

        var classRooms = await _context.ClassRooms
            .AsNoTracking()
            .Include(c => c.Students)
            .ToListAsync(cancellationToken);

        var teacherIds = teachers.Select(t => t.Id).ToList();

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => teacherIds.Contains(n.UserId))
            .ToListAsync(cancellationToken);

        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => teacherIds.Contains(c.TeacherId))
            .ToListAsync(cancellationToken);

        var rows = teachers.Select(t =>
        {
            var classes = classRooms
                .Where(c => c.TeacherId == t.Id)
                .Select(c => string.IsNullOrWhiteSpace(c.Grade)
                    ? c.Name
                    : $"Grade {c.Grade}{c.Section}")
                .ToList();

            var studentCount = classRooms
                .Where(c => c.TeacherId == t.Id)
                .Sum(c => c.Students.Count);

            var alertCount = notifications.Count(n => n.UserId == t.Id && IsFlaggedNotification(n));
            var subject = courses.FirstOrDefault(c => c.TeacherId == t.Id)?.Subject ?? string.Empty;

            return new AdminTeacherRowDto
            {
                Id = t.Id,
                Name = t.Name,
                Username = t.Username,
                Subject = subject,
                Classes = classes,
                StudentCount = studentCount,
                AlertCount = alertCount,
                Status = t.IsActive ? "Active" : "Inactive"
            };
        }).ToList();

        var dto = new AdminTeacherModuleSummaryDto
        {
            TotalTeachers = teachers.Count,
            ActiveTeachers = teachers.Count(t => t.IsActive),
            TotalStudents = classRooms.Sum(c => c.Students.Count),
            TotalAlerts = rows.Sum(r => r.AlertCount),
            Teachers = rows
        };

        return Ok(dto);
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<AdminNotificationFeedDto>> GetNotifications(CancellationToken cancellationToken)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(80)
            .ToListAsync(cancellationToken);

        var items = notifications.Select(n => new AdminNotificationItemDto
        {
            Id = n.Id,
            Title = n.Title,
            Severity = GetNotificationSeverity(n.Title, n.Message, n.Type),
            Description = n.Message,
            CreatedAt = n.CreatedAt,
            IsNew = !n.IsRead
        }).ToList();

        return Ok(new AdminNotificationFeedDto { Items = items });
    }

    [HttpGet("help")]
    public ActionResult<List<AdminHelpItemDto>> GetHelpItems()
    {
        return Ok(new List<AdminHelpItemDto>());
    }

    [HttpGet("support")]
    public ActionResult<List<AdminSupportTicketDto>> GetSupportTickets()
    {
        return Ok(new List<AdminSupportTicketDto>());
    }

    private static List<WeeklyPointDto> BuildWeekly(List<MessageRow> messageRows)
    {
        var today = DateTime.UtcNow.Date;
        var points = new List<WeeklyPointDto>();

        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var items = messageRows.Where(m => m.SentOn.Date == day).ToList();
            var total = items.Count;
            var flagged = items.Count(m => IsFlaggedMessage(m.Content));
            var safe = total - flagged;

            points.Add(new WeeklyPointDto
            {
                Day = day.ToString("ddd"),
                Safe = total == 0 ? 0 : (int)Math.Round((safe * 100d) / total),
                Flagged = total == 0 ? 0 : (int)Math.Round((flagged * 100d) / total),
                Blocked = 0
            });
        }

        return points;
    }

    private async Task<List<AdminAlertDto>> BuildRecentAlertsAsync(CancellationToken cancellationToken)
    {
        var unapproved = await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive && !u.IsApproved)
            .OrderByDescending(u => u.AddedOn)
            .Take(5)
            .ToListAsync(cancellationToken);

        var inactive = await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsActive)
            .OrderByDescending(u => u.AddedOn)
            .Take(5)
            .ToListAsync(cancellationToken);

        var alerts = new List<AdminAlertDto>();

        alerts.AddRange(unapproved.Select(u => new AdminAlertDto
        {
            Title = u.Name,
            Description = "Pending approval request",
            Severity = "Warning",
            CreatedAt = u.AddedOn
        }));

        alerts.AddRange(inactive.Select(u => new AdminAlertDto
        {
            Title = u.Name,
            Description = "User inactive",
            Severity = "Blocked",
            CreatedAt = u.AddedOn
        }));

        return alerts
            .OrderByDescending(a => a.CreatedAt)
            .Take(8)
            .ToList();
    }

    private static bool IsFlaggedMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        return FlagKeywords.Any(k => content.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsFlaggedNotification(Notification notification)
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

    private static string GetNotificationSeverity(string? title, string? message, string? type)
    {
        var joined = string.Join(" ", title, message, type);
        if (joined.Contains("block", StringComparison.OrdinalIgnoreCase)) return "Blocked";
        if (joined.Contains("warn", StringComparison.OrdinalIgnoreCase) || joined.Contains("alert", StringComparison.OrdinalIgnoreCase)) return "Warning";
        return "Safe";
    }

    private sealed record MessageRow(string Content, DateTime SentOn);

    private sealed record MessageRowWithUsers(int FromId, int ToId, string Content, DateTime SentOn);

    private sealed record MessageRowWithFrom(int FromId, string Content, DateTime SentOn);
}
