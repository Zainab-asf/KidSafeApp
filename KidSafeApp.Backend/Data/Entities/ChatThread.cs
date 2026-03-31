using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Backend.Data.Entities
{
    public class ChatThread
    {
        [Key]
        public int Id { get; set; }

        public int OwnerUserId { get; set; }
        public virtual User OwnerUser { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual ICollection<ThreadMessage> Messages { get; set; } = new List<ThreadMessage>();
    }
}
