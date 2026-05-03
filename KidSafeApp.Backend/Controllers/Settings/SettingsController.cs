using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers.Settings
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly DataContext _context;

        public SettingsController(DataContext context)
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

            var preferredRole = string.IsNullOrWhiteSpace(role) ? "Child" : role.Trim();

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
                Username = $"{preferredRole.ToLowerInvariant()}_settings_demo",
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

        // GET: api/settings
        [HttpGet]
        public async Task<ActionResult<UserSettingsDto>> GetUserSettings([FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (settings == null)
            {
                // Create default settings if they don't exist
                settings = new UserSettings
                {
                    UserId = userId,
                    ThemePreference = "light",
                    LanguagePreference = "en",
                    ParentEmail = string.Empty,
                    Bio = string.Empty,
                    AvatarUrl = string.Empty
                };

                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Ok(MapToDto(settings));
        }

        // PUT: api/settings
        [HttpPut]
        public async Task<ActionResult<UserSettingsDto>> UpdateUserSettings(UserSettingsDto dto, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    ParentEmail = string.Empty,
                    Bio = string.Empty,
                    AvatarUrl = string.Empty
                };
                _context.UserSettings.Add(settings);
            }

            // Update all properties from DTO
            settings.IsProfilePublic = dto.IsProfilePublic;
            settings.AllowMessageRequests = dto.AllowMessageRequests;
            settings.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;
            settings.PushNotificationsEnabled = dto.PushNotificationsEnabled;
            settings.AchievementNotifications = dto.AchievementNotifications;
            settings.MessageNotifications = dto.MessageNotifications;
            settings.CourseReminderNotifications = dto.CourseReminderNotifications;
            settings.ThemePreference = dto.ThemePreference ?? "light";
            settings.LanguagePreference = dto.LanguagePreference ?? "en";
            settings.CompactMode = dto.CompactMode;
            settings.StrictContentFilter = dto.StrictContentFilter;
            settings.BlockAdultContent = dto.BlockAdultContent;
            settings.ParentEmail = dto.ParentEmail;
            settings.RequireParentApprovalForMessages = dto.RequireParentApprovalForMessages;
            settings.ScreenTimeLimit = dto.ScreenTimeLimit;
            settings.Bio = dto.Bio;
            settings.AvatarUrl = dto.AvatarUrl;
            settings.UpdatedAt = DateTime.UtcNow;

            _context.UserSettings.Update(settings);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(settings));
        }

        // PUT: api/settings/notifications
        [HttpPut("notifications")]
        public async Task<ActionResult<UserSettingsDto>> UpdateNotificationSettings(NotificationSettingsRequest request, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    ParentEmail = string.Empty,
                    Bio = string.Empty,
                    AvatarUrl = string.Empty
                };
                _context.UserSettings.Add(settings);
            }

            settings.EmailNotificationsEnabled = request.EmailNotificationsEnabled;
            settings.PushNotificationsEnabled = request.PushNotificationsEnabled;
            settings.AchievementNotifications = request.AchievementNotifications;
            settings.MessageNotifications = request.MessageNotifications;
            settings.CourseReminderNotifications = request.CourseReminderNotifications;
            settings.UpdatedAt = DateTime.UtcNow;

            _context.UserSettings.Update(settings);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(settings));
        }

        // PUT: api/settings/privacy
        [HttpPut("privacy")]
        public async Task<ActionResult<UserSettingsDto>> UpdatePrivacySettings(PrivacySettingsRequest request, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    ParentEmail = string.Empty,
                    Bio = string.Empty,
                    AvatarUrl = string.Empty
                };
                _context.UserSettings.Add(settings);
            }

            settings.IsProfilePublic = request.IsProfilePublic;
            settings.AllowMessageRequests = request.AllowMessageRequests;
            settings.BlockAdultContent = request.BlockAdultContent;
            settings.StrictContentFilter = request.StrictContentFilter;
            settings.UpdatedAt = DateTime.UtcNow;

            _context.UserSettings.Update(settings);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(settings));
        }

        // PUT: api/settings/display
        [HttpPut("display")]
        public async Task<ActionResult<UserSettingsDto>> UpdateDisplaySettings(DisplaySettingsRequest request, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    ParentEmail = string.Empty,
                    Bio = string.Empty,
                    AvatarUrl = string.Empty
                };
                _context.UserSettings.Add(settings);
            }

            settings.ThemePreference = request.ThemePreference ?? "light";
            settings.LanguagePreference = request.LanguagePreference ?? "en";
            settings.CompactMode = request.CompactMode;
            settings.UpdatedAt = DateTime.UtcNow;

            _context.UserSettings.Update(settings);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(settings));
        }

        // PUT: api/settings/profile
        [HttpPut("profile")]
        public async Task<ActionResult<UserSettingsDto>> UpdateProfileSettings(ProfileSettingsRequest request, [FromQuery] string? role = null, CancellationToken cancellationToken = default)
        {
            var userId = await GetCurrentUserIdAsync(role, cancellationToken);
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    ParentEmail = string.Empty,
                    Bio = string.Empty,
                    AvatarUrl = string.Empty
                };
                _context.UserSettings.Add(settings);
            }

            settings.Bio = request.Bio ?? string.Empty;
            settings.AvatarUrl = request.AvatarUrl ?? string.Empty;
            settings.UpdatedAt = DateTime.UtcNow;

            _context.UserSettings.Update(settings);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(settings));
        }

        private UserSettingsDto MapToDto(UserSettings settings)
        {
            return new UserSettingsDto
            {
                Id = settings.Id,
                IsProfilePublic = settings.IsProfilePublic,
                AllowMessageRequests = settings.AllowMessageRequests,
                EmailNotificationsEnabled = settings.EmailNotificationsEnabled,
                PushNotificationsEnabled = settings.PushNotificationsEnabled,
                AchievementNotifications = settings.AchievementNotifications,
                MessageNotifications = settings.MessageNotifications,
                CourseReminderNotifications = settings.CourseReminderNotifications,
                ThemePreference = settings.ThemePreference,
                LanguagePreference = settings.LanguagePreference,
                CompactMode = settings.CompactMode,
                StrictContentFilter = settings.StrictContentFilter,
                BlockAdultContent = settings.BlockAdultContent,
                ParentEmail = settings.ParentEmail,
                RequireParentApprovalForMessages = settings.RequireParentApprovalForMessages,
                ScreenTimeLimit = settings.ScreenTimeLimit,
                Bio = settings.Bio,
                AvatarUrl = settings.AvatarUrl
            };
        }
    }

    public class NotificationSettingsRequest
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool AchievementNotifications { get; set; }
        public bool MessageNotifications { get; set; }
        public bool CourseReminderNotifications { get; set; }
    }

    public class PrivacySettingsRequest
    {
        public bool IsProfilePublic { get; set; }
        public bool AllowMessageRequests { get; set; }
        public bool BlockAdultContent { get; set; }
        public bool StrictContentFilter { get; set; }
    }

    public class DisplaySettingsRequest
    {
        public string ThemePreference { get; set; } = string.Empty;
        public string LanguagePreference { get; set; } = string.Empty;
        public bool CompactMode { get; set; }
    }

    public class ProfileSettingsRequest
    {
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
