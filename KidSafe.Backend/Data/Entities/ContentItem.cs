namespace KidSafe.Backend.Data.Entities;

/// <summary>
/// Teacher-uploaded learning content.
/// Type: Note | PDF | Assignment | Announcement | Link
/// </summary>
public class ContentItem
{
    public int      Id          { get; set; }
    public int      ClassId     { get; set; }
    public int      TeacherId   { get; set; }
    public string   Title       { get; set; } = string.Empty;
    public string   Description { get; set; } = string.Empty;
    /// <summary>Note | PDF | Assignment | Announcement | Link</summary>
    public string   Type        { get; set; } = "Note";
    /// <summary>Relative server path for file uploads, or URL for links.</summary>
    public string?  FilePath    { get; set; }
    public string?  FileType    { get; set; }   // "pdf" | "docx" | "png" etc.
    public long     FileSize    { get; set; }    // bytes
    public DateTime? DueDate    { get; set; }    // assignments only
    public int      MaxPoints   { get; set; }    // assignments only
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    public Class   Class   { get; set; } = null!;
    public User    Teacher { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = [];
}
