using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("ClassRoom")]
    public class ClassRoom
    {
        [Key]
        public int Id { get; set; }

        [Required, Unicode(false), MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [Unicode(false), MaxLength(30)]
        public string Grade { get; set; } = string.Empty;

        [Unicode(false), MaxLength(30)]
        public string Section { get; set; } = string.Empty;

        public int? TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public User? Teacher { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ClassRoomStudent> Students { get; set; } = new List<ClassRoomStudent>();

        public ICollection<ClassRoomCourseAssignment> CourseAssignments { get; set; } = new List<ClassRoomCourseAssignment>();
    }
}
