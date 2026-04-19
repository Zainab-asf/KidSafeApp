using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("ClassRoomCourseAssignment")]
    public class ClassRoomCourseAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; } = default!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
