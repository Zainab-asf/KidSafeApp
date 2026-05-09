namespace KidSafe.Shared.Interfaces;

public interface IChatHubClient
{
    Task ReceiveMessage(int senderId, string senderName, string message, string label);
    Task UserTyping(string userId, string userName);
    Task FlaggedMessageAlert(int senderId, string senderName, string maskedMessage, string label, double score);
}
