namespace KidSafe.Backend.Data.Entities;

public class Reward
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int    Points       { get; set; } = 0;
    public string BadgeLevel   { get; set; } = "Safe Chatter";
    public int    SafeMessages { get; set; } = 0;
    public string Badges       { get; set; } = "[]";
    public User User { get; set; } = null!;
}
