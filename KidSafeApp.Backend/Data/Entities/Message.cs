using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public long Id { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }

        [Required, MaxLength(500)]
        public string Content { get; set; } = string.Empty;
        public DateTime SentOn { get; set; }
        public bool IsRead { get; set; } = false;

        [ForeignKey(nameof(Message.FromId))]
        public virtual User FromUser { get; set; } = null!;

        [ForeignKey(nameof(Message.ToId))]
        public virtual User ToUser { get; set; } = null!;
    }
}
