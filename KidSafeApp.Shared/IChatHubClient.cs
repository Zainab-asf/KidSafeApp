using KidSafeApp.Shared.DTOs;

namespace KidSafeApp.Shared;

public interface IChatHubClient
{
   Task UserConnected(UserDto user);
	Task OnlineUsersList(IEnumerable<UserDto> users);
	Task UserIsOnline(int userId);
	Task MessageRecieved(MessageDto message);
}
