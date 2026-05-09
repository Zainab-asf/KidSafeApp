namespace KidSafe.Shared.DTOs;

public class DashboardStats
{
    // New SDD labels: Watch (was Flagged) | Review (was Blocked)
    public int TotalWatch    { get; set; }
    public int TotalReview   { get; set; }
    public int TotalFlagged  { get; set; }  // combined Watch + Review
    public int TotalChildren { get; set; }
    public int PendingTeachers  { get; set; }
    public int TotalComplaints  { get; set; }
    public int TotalReports     { get; set; }
    public List<ActivityItem> RecentActivity { get; set; } = new();
}

public class ActivityItem
{
    public string SenderName { get; set; } = string.Empty;
    public string Label      { get; set; } = string.Empty;
    public double Score      { get; set; }
    public DateTime Timestamp { get; set; }
}

public class FlaggedMessageItem
{
    public int    Id            { get; set; }
    public int    SenderId      { get; set; }
    public string SenderName    { get; set; } = string.Empty;
    public string MaskedMessage { get; set; } = string.Empty;
    public string Label         { get; set; } = string.Empty;
    public double Score         { get; set; }
    public DateTime Timestamp   { get; set; }
}
