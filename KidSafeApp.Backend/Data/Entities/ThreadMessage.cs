using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Backend.Data.Entities
{
    public class ThreadMessage
    {
        [Key]
        public long Id { get; set; }

        public int ChatThreadId { get; set; }
        public virtual ChatThread ChatThread { get; set; } = null!;

        public int SenderUserId { get; set; }
        public virtual User SenderUser { get; set; } = null!;

        public int? RecipientUserId { get; set; }
        public virtual User? RecipientUser { get; set; }

        [Required, MaxLength(2048)]
        public string Text { get; set; } = string.Empty;

        public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;

        public bool IsFlagged { get; set; }

        [MaxLength(200)]
        public string? FlagReason { get; set; }
    }
}
