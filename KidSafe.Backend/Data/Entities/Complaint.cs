namespace KidSafe.Backend.Data.Entities;

public class Complaint
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DateFiled { get; set; } = DateTime.UtcNow;
    /// <summary>submitted | underReview | resolved | withdrawn</summary>
    public string Status { get; set; } = "submitted";
    public int? LinkedReportId { get; set; }

    public User User { get; set; } = null!;
}
