using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("Course")]
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required, Unicode(false), MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Unicode(false), MaxLength(1000)]
        public string? Description { get; set; }

        [Unicode(false), MaxLength(60)]
        public string? Subject { get; set; }

        public int DifficultyLevel { get; set; } = 1;

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public User Teacher { get; set; } = default!;

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CourseLesson> CourseLessons { get; set; } = new List<CourseLesson>();
    }
}
