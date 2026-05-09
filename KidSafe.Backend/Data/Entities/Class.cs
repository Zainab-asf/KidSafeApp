namespace KidSafe.Backend.Data.Entities;

public class Class
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = string.Empty;  // e.g. "Grade 5"
    public string   Section     { get; set; } = "A";
    public string   Subject     { get; set; } = string.Empty;  // e.g. "Mathematics"
    public int?     TeacherId   { get; set; }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    public User?                   Teacher      { get; set; }
    public ICollection<ClassStudent>  Students  { get; set; } = [];
    public ICollection<ContentItem>   Content   { get; set; } = [];
    public ICollection<ChatMessage>   Messages  { get; set; } = [];
}
