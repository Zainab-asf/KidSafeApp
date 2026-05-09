namespace KidSafe.Shared.Interfaces;

public interface IChatHubServer
{
    Task JoinParentRoom();
    Task SendTypingIndicator(int receiverId);
}
