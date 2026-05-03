using System.IdentityModel.Tokens.Jwt;
using KidSafeApp.StateManagement;
using KidSafeApp.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KidSafeApp.Services;
public sealed class AuthenticationService
{
    private readonly AuthenticationState _authenticationState;
    private readonly RoleState _roleState;
    private readonly AdminSessionState _adminSessionState;
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;

    public AuthenticationService(
        AuthenticationState authenticationState,
        RoleState roleState,
        AdminSessionState adminSessionState,
        NavigationManager navigationManager,
        IJSRuntime jsRuntime)
    {
        _authenticationState = authenticationState;
        _roleState = roleState;
        _adminSessionState = adminSessionState;
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
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
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.removeFromStorage", AuthenticationState.AuthStoreKey);
        }
        catch { /* ignore if JSRuntime not available */ }
        _navigationManager.NavigateTo("/", replace: true);
    }

    public bool IsAuthenticated => 
        _authenticationState?.IsAuthenticated ?? false;

    public UserDto? CurrentUser => 
        _authenticationState?.User;

    public string? CurrentToken => 
        _authenticationState?.Token;
}
