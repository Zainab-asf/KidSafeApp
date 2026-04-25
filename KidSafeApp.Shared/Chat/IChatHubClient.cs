using KidSafeApp.Shared.DTOs.Auth;
using KidSafeApp.Shared.DTOs.Chat;

namespace KidSafeApp.Shared.Chat;

public interface IChatHubClient
{
   Task UserConnected(UserDto user);
	Task OnlineUsersList(IEnumerable<UserDto> users);
	Task UserIsOnline(int userId);
	Task MessageRecieved(MessageDto message);
    Task RosterUpdated(int classRoomId);
}
