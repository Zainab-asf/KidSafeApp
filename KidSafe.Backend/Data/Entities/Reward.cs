namespace KidSafe.Backend.Data.Entities;

public class Reward
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Points { get; set; } = 0;
    public string Badges { get; set; } = "[]"; // JSON array of badge names
    public User User { get; set; } = null!;
}
