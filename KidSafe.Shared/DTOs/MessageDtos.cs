namespace KidSafe.Shared.DTOs;

public record SendMessageRequest(int ReceiverId, string Message);
public record MessageResult(string Status, string? MaskedMessage, string Label, double Score);

public class ChatMessage
{
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Label { get; set; } = "safe";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
