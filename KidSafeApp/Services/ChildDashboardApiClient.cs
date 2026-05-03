using KidSafeApp.Shared.DTOs.Dashboard;
using KidSafeApp.StateManagement;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class ChildDashboardApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _authenticationState;

    public ChildDashboardApiClient(HttpClient httpClient, AuthenticationState authenticationState)
    {
        _httpClient = httpClient;
        _authenticationState = authenticationState;
    }

    public async Task<ChildDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<ChildDashboardDto>("api/dashboard/child", cancellationToken)
            ?? new ChildDashboardDto();
    }

    private void ApplyBearerToken()
    {
        if (string.IsNullOrWhiteSpace(_authenticationState.Token))
        {
            return;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationState.Token);
    }
}
