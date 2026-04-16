namespace KidSafeApp.Shared.DTOs
{
    public class UserSettingsDto
    {
        public int Id { get; set; }
        public bool IsProfilePublic { get; set; }
        public bool AllowMessageRequests { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool AchievementNotifications { get; set; }
        public bool MessageNotifications { get; set; }
        public bool CourseReminderNotifications { get; set; }
        public string ThemePreference { get; set; }
        public string LanguagePreference { get; set; }
        public bool CompactMode { get; set; }
        public bool StrictContentFilter { get; set; }
        public bool BlockAdultContent { get; set; }
        public string ParentEmail { get; set; }
        public bool RequireParentApprovalForMessages { get; set; }
        public int ScreenTimeLimit { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
    }
}
