using KidSafeApp.States;
using Microsoft.AspNetCore.Components;
using System.IdentityModel.Tokens.Jwt;

namespace KidSafeApp.Services;

public sealed class AuthenticationService
{
    private readonly AuthenticationState _authState;
    private readonly RoleState _roleState;
    private readonly NavigationManager _nav;

    public AuthenticationService(AuthenticationState authState, RoleState roleState, NavigationManager nav)
    {
        _authState = authState;
        _roleState = roleState;
        _nav = nav;
    }

    public bool IsAuthenticated => _authState.IsAuthenticated && !string.IsNullOrWhiteSpace(_authState.Token);

    public string? CurrentToken => _authState.Token;

    public Task<bool> IsTokenValidAsync()
    {
        var token = _authState.Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var exp = jwt.Payload.Exp;
            if (exp is null)
            {
                return Task.FromResult(true);
            }

            var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp.Value).UtcDateTime;
            return Task.FromResult(expUtc > DateTime.UtcNow.AddMinutes(1));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task LogoutAsync()
    {
        _roleState.Reset();
        _authState.UnLoadState();
        _nav.NavigateTo("/", replace: true);
        return Task.CompletedTask;
    }
}

