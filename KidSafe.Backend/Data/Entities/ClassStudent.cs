namespace KidSafe.Backend.Data.Entities;

/// <summary>Junction: many students ↔ many classes</summary>
public class ClassStudent
{
    public int ClassId   { get; set; }
    public int StudentId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Class Class   { get; set; } = null!;
    public User  Student { get; set; } = null!;
}
