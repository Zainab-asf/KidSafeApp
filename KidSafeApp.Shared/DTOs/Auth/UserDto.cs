namespace KidSafeApp.Shared.DTOs.Auth
{
    public class UserDto
    {
        public UserDto(int id, string name, bool isOnline = false, string role = "Child")
        {
            Id = id;
            Name = name;
            IsOnline = isOnline;
            Role = role;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
        public string Role { get; set; } = "Child";
        public bool IsSelected { get; set; }
    }
}
