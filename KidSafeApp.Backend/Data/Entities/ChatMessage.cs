using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Backend.Data.Entities;

public class ChatMessage
{
	[Key]
	public long Id { get; set; }

   public int SenderUserId { get; set; }
	public User SenderUser { get; set; } = null!;

   public int? RecipientUserId { get; set; }
	public User? RecipientUser { get; set; }

	[Required, MaxLength(2048)]
	public string Text { get; set; } = string.Empty;

	public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;

	public bool IsFlagged { get; set; }

	[MaxLength(200)]
	public string? FlagReason { get; set; }
}
