namespace KidSafe.Backend.Data.Entities;

/// <summary>Junction: one parent ↔ many children</summary>
public class ParentChild
{
    public int ParentId { get; set; }
    public int ChildId  { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    public User Parent { get; set; } = null!;
    public User Child  { get; set; } = null!;
}
