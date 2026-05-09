namespace KidSafe.Backend.Data.Entities;

public class Submission
{
    public int      Id            { get; set; }
    public int      ContentItemId { get; set; }  // assignment
    public int      StudentId     { get; set; }
    public string?  FilePath      { get; set; }
    public string?  TextAnswer    { get; set; }
    public int?     Points        { get; set; }  // null = not graded yet
    public string   Feedback      { get; set; } = string.Empty;
    public DateTime SubmittedAt   { get; set; } = DateTime.UtcNow;

    public ContentItem Assignment { get; set; } = null!;
    public User        Student    { get; set; } = null!;
}
