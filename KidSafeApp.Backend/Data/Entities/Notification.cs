using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required, Unicode(false), MaxLength(100)]
        public string Title { get; set; }

        [Required, Unicode(false), MaxLength(500)]
        public string Message { get; set; }

        [Unicode(false), MaxLength(50)]
        public string Type { get; set; } = "Info"; // Info, Achievement, Warning, Alert

        [Unicode(false), MaxLength(255)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public DateTime ExpiresAt { get; set; } // Auto-delete after this date
    }
}
