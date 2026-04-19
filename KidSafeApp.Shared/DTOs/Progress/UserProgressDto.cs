namespace KidSafeApp.Shared.DTOs.Progress
{
    public class UserProgressDto
    {
        public int Id { get; set; }
        public string CourseTitle { get; set; }
        public int LessonsCompleted { get; set; }
        public int TotalLessons { get; set; }
        public int PercentageComplete { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; }
        public int Streak { get; set; }
        public int TotalPoints { get; set; }
        public DateTime LastAccessedDate { get; set; }
    }
}
