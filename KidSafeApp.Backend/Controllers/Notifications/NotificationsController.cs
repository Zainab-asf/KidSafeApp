using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers.Notifications
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly DataContext _context;

        public NotificationsController(DataContext context)
        {
            _context = context;
        }

        private async Task<int> GetCurrentUserIdAsync(string? role, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim?.Value, out var userIdFromClaim) && userIdFromClaim > 0)
            {
                return userIdFromClaim;
            }

            var preferredRole = string.IsNullOrWhiteSpace(role) ? "Parent" : role.Trim();

            var existingUserId = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == preferredRole && u.IsActive)
                .OrderBy(u => u.Id)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingUserId > 0)
            {
                return existingUserId;
            }

            var demoUser = new User
            {
                Name = $"{preferredRole} Demo",
                Username = $"{preferredRole.ToLowerInvariant()}_notifications_demo",
                Password = "demo",
                Role = preferredRole,
                IsApproved = true,
                IsActive = true,
                AddedOn = DateTime.UtcNow
            };

            _context.Users.Add(demoUser);
            await _context.SaveChangesAsync(cancellationToken);
            return demoUser.Id;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            IQueryable<Notification> query = _context.Notifications
                .Where(n => n.UserId == userId && n.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(n => n.CreatedAt);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .Select(n => new NotificationDto
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
                })
                .ToListAsync(cancellationToken);

            return Ok(notifications);
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount([FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.ExpiresAt > DateTime.UtcNow)
                .CountAsync(cancellationToken);

            return Ok(unreadCount);
        }

        // PUT: api/notifications/5/mark-as-read
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        // PUT: api/notifications/mark-all-as-read
        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            _context.Notifications.UpdateRange(unreadNotifications);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        // DELETE: api/notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

            if (notification == null)
                return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        // DELETE: api/notifications/clear-expired
        [HttpDelete("clear-expired")]
        public async Task<IActionResult> ClearExpiredNotifications([FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var expiredNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            _context.Notifications.RemoveRange(expiredNotifications);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { deletedCount = expiredNotifications.Count });
        }

        // POST: api/notifications/send (admin only - for testing)
        [HttpPost("send")]
        public async Task<ActionResult<NotificationDto>> SendNotification(CreateNotificationRequest request, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var notification = new Notification
            {
                UserId = userId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type ?? "Info",
                ActionUrl = request.ActionUrl,
                ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetNotifications), new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                ActionUrl = notification.ActionUrl,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                ExpiresAt = notification.ExpiresAt
            });
        }
    }

    public class CreateNotificationRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string ActionUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
