using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("CourseAssignment")]
    public class CourseAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        [Required]
        public int ChildId { get; set; }

        [ForeignKey(nameof(ChildId))]
        public User Child { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
