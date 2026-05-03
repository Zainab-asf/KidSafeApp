using KidSafeApp.Shared.DTOs.Dashboard;
using KidSafeApp.StateManagement;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class TeacherDashboardApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _authenticationState;

    public TeacherDashboardApiClient(HttpClient httpClient, AuthenticationState authenticationState)
    {
        _httpClient = httpClient;
        _authenticationState = authenticationState;
    }

    public async Task<TeacherDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<TeacherDashboardDto>("api/dashboard/teacher", cancellationToken)
            ?? new TeacherDashboardDto();
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
