using KidSafeApp.Shared.DTOs;

namespace KidSafeApp.Shared;

public interface IChatHubServer
{
	Task SetUserOnline(UserDto user);
}
