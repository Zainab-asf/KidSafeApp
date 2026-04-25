using System.IdentityModel.Tokens.Jwt;
using KidSafeApp.States;
using KidSafeApp.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components;

namespace KidSafeApp.Services;

public sealed class AuthenticationService
{
    private readonly AuthenticationState _authenticationState;
    private readonly RoleState _roleState;
    private readonly AdminSessionState _adminSessionState;
    private readonly NavigationManager _navigationManager;

    public AuthenticationService(
        AuthenticationState authenticationState,
        RoleState roleState,
        AdminSessionState adminSessionState,
        NavigationManager navigationManager)
    {
        _authenticationState = authenticationState;
        _roleState = roleState;
        _adminSessionState = adminSessionState;
        _navigationManager = navigationManager;
    }

    public async Task<bool> IsTokenValidAsync()
    {
        if (_authenticationState is null || string.IsNullOrWhiteSpace(_authenticationState.Token))
        {
            return false;
        }

        try
        {
            var jwt = new JwtSecurityToken(_authenticationState.Token);
            if (jwt.ValidTo <= DateTime.UtcNow)
            {
                await LogoutAsync();
                return false;
            }
            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        _adminSessionState.SignOut();
        _authenticationState.UnLoadState();
        _roleState.Reset();
        _navigationManager.NavigateTo("/", replace: true);
    }

    public bool IsAuthenticated => 
        _authenticationState?.IsAuthenticated ?? false;

    public UserDto? CurrentUser => 
        _authenticationState?.User;

    public string? CurrentToken => 
        _authenticationState?.Token;
}
