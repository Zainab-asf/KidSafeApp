namespace KidSafeApp.Shared.DTOs.Auth
{
    public class UserDto
    {
        public UserDto(int id, string name, bool isOnline = false, string role = "Child", string? avatarUrl = null, int unreadCount = 0)
        {
            Id = id;
            Name = name;
            IsOnline = isOnline;
            Role = role;
            AvatarUrl = avatarUrl;
            UnreadCount = unreadCount;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
        public string Role { get; set; } = "Child";
        public bool IsSelected { get; set; }
        public string? AvatarUrl { get; set; }
        public int UnreadCount { get; set; }
    }
}
