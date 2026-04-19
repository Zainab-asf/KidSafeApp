using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("UserSettings")]
    public class UserSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        // Privacy Settings
        public bool IsProfilePublic { get; set; } = false;

        public bool AllowMessageRequests { get; set; } = true;

        // Notification Settings
        public bool EmailNotificationsEnabled { get; set; } = true;

        public bool PushNotificationsEnabled { get; set; } = true;

        public bool AchievementNotifications { get; set; } = true;

        public bool MessageNotifications { get; set; } = true;

        public bool CourseReminderNotifications { get; set; } = true;

        // Display Settings
        [Unicode(false), MaxLength(20)]
        public string ThemePreference { get; set; } = "light"; // light, dark, system

        [Unicode(false), MaxLength(20)]
        public string LanguagePreference { get; set; } = "en"; // en, es, fr, etc.

        public bool CompactMode { get; set; } = false;

        // Content Filter
        public bool StrictContentFilter { get; set; } = true;

        public bool BlockAdultContent { get; set; } = true;

        // Parental Controls (if user is a child)
        [Unicode(false), MaxLength(100)]
        public string ParentEmail { get; set; } = string.Empty;

        public bool RequireParentApprovalForMessages { get; set; } = false;

        public int ScreenTimeLimit { get; set; } = 180; // minutes per day

        // Profile
        [Unicode(false), MaxLength(500)]
        public string Bio { get; set; } = string.Empty;

        [Unicode(false), MaxLength(100)]
        public string AvatarUrl { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
