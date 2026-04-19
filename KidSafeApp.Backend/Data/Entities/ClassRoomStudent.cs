using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("ClassRoomStudent")]
    public class ClassRoomStudent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; } = default!;

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; } = default!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
