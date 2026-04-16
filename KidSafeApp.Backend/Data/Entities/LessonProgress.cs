using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("LessonProgress")]
    public class LessonProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseLessonId { get; set; }

        [ForeignKey(nameof(CourseLessonId))]
        public CourseLesson CourseLesson { get; set; } = default!;

        [Required]
        public int ChildId { get; set; }

        [ForeignKey(nameof(ChildId))]
        public User Child { get; set; } = default!;

        public bool IsStarted { get; set; }

        public bool IsCompleted { get; set; }

        public int PercentageComplete { get; set; }

        public int TimeSpentSeconds { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
