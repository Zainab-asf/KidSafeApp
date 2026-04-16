using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("UserProgress")]
    public class UserProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required, Unicode(false), MaxLength(100)]
        public string CourseTitle { get; set; }

        public int LessonsCompleted { get; set; }

        public int TotalLessons { get; set; }

        public int PercentageComplete { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        public string Status { get; set; } = "In Progress"; // In Progress, Completed, Not Started

        public int Streak { get; set; } = 0; // Days in a row

        public int TotalPoints { get; set; } = 0;

        public DateTime LastAccessedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
