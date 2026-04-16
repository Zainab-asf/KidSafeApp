using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("CourseLesson")]
    public class CourseLesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        [Required]
        public int LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public Lesson Lesson { get; set; } = default!;

        public int SortOrder { get; set; }

        [Unicode(false), MaxLength(300)]
        public string? PdfUrl { get; set; }

        public bool IsPublished { get; set; } = false;

        public ICollection<LessonProgress> LessonProgressItems { get; set; } = new List<LessonProgress>();
    }
}
