using KidSafeApp.Shared.DTOs.Auth;

namespace KidSafeApp.Shared.Chat;

public interface IChatHubServer
{
	Task SetUserOnline(UserDto user);
}
