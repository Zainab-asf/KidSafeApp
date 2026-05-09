using System.Net.Http.Headers;

namespace KidSafe.MAUI.Services;

/// <summary>
/// Injects the JWT Bearer token into every outbound HTTP request.
/// Registered in DI as a DelegatingHandler so ApiService stays stateless.
/// </summary>
public sealed class AuthDelegatingHandler : DelegatingHandler
{
    private readonly AuthStateService _auth;

    public AuthDelegatingHandler(AuthStateService auth) => _auth = auth;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _auth.CurrentUser?.Token;
        if (!string.IsNullOrEmpty(token) && token is not ("pending" or "disabled"))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
