using KidSafeApp.Shared.DTOs.Notifications;
using KidSafeApp.Shared.DTOs.Settings;
using KidSafeApp.Shared.DTOs.Dashboard;
using KidSafeApp.StateManagement;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class ParentDashboardApiClient
{
    private readonly HttpClient _client;
    private readonly AuthenticationState _authState;

    public ParentDashboardApiClient(HttpClient client, AuthenticationState authState)
    {
        _client = client;
        _authState = authState;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var url = unreadOnly
            ? "api/notifications?role=Parent&unreadOnly=true"
            : "api/notifications?role=Parent";

        var data = await _client.GetFromJsonAsync<List<NotificationDto>>(url, cancellationToken);
        return (data ?? new List<NotificationDto>())
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<ParentDashboardDto?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _client.GetFromJsonAsync<ParentDashboardDto>("api/dashboard/parent", cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetFlaggedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await GetNotificationsAsync(unreadOnly: false, cancellationToken);
        return notifications.Where(IsFlaggedSignal).ToList();
    }

    public async Task MarkNotificationAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var response = await _client.PutAsync($"api/notifications/{id}/mark-as-read?role=Parent", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAllNotificationsAsReadAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var response = await _client.PutAsync("api/notifications/mark-all-as-read?role=Parent", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<UserSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _client.GetFromJsonAsync<UserSettingsDto>("api/settings?role=Parent", cancellationToken);
    }

    public async Task<UserSettingsDto?> SaveSettingsAsync(UserSettingsDto settings, CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var response = await _client.PutAsJsonAsync("api/settings?role=Parent", settings, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserSettingsDto>(cancellationToken);
    }

    public static bool IsFlaggedSignal(NotificationDto notification)
    {
        var title = notification.Title ?? string.Empty;
        var message = notification.Message ?? string.Empty;
        var type = notification.Type ?? string.Empty;

        return type.Contains("warn", StringComparison.OrdinalIgnoreCase)
            || type.Contains("alert", StringComparison.OrdinalIgnoreCase)
            || title.Contains("flag", StringComparison.OrdinalIgnoreCase)
            || title.Contains("block", StringComparison.OrdinalIgnoreCase)
            || message.Contains("flag", StringComparison.OrdinalIgnoreCase)
            || message.Contains("block", StringComparison.OrdinalIgnoreCase)
            || message.Contains("inappropriate", StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyBearerToken()
    {
        if (!string.IsNullOrWhiteSpace(_authState.Token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _authState.Token);
        }
    }
}
