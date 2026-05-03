using KidSafeApp.Shared.DTOs.Notifications;
using KidSafeApp.Shared.DTOs.Settings;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class ParentDashboardApiClient
{
    private readonly HttpClient _http;

    public ParentDashboardApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(CancellationToken cancellationToken = default)
        => await _http.GetFromJsonAsync<List<NotificationDto>>("api/notifications?role=Parent", cancellationToken) ?? new List<NotificationDto>();

    public async Task<IReadOnlyList<NotificationDto>> GetFlaggedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var all = await GetNotificationsAsync(cancellationToken);
        return all.Where(IsFlaggedSignal).ToList();
    }

    public Task MarkNotificationAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
        => _http.PutAsync($"api/notifications/{notificationId}/mark-as-read?role=Parent", content: null, cancellationToken);

    public Task MarkAllNotificationsAsReadAsync(CancellationToken cancellationToken = default)
        => _http.PutAsync("api/notifications/mark-all-as-read?role=Parent", content: null, cancellationToken);

    public Task<UserSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsync<UserSettingsDto>("api/settings?role=Parent", cancellationToken);

    public async Task<UserSettingsDto?> SaveSettingsAsync(UserSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var resp = await _http.PutAsJsonAsync("api/settings?role=Parent", dto, cancellationToken);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<UserSettingsDto>(cancellationToken: cancellationToken);
    }

    public static bool IsFlaggedSignal(NotificationDto n)
    {
        var title = n.Title ?? string.Empty;
        var type = n.Type ?? string.Empty;
        return title.Contains("flag", StringComparison.OrdinalIgnoreCase)
            || title.Contains("block", StringComparison.OrdinalIgnoreCase)
            || type.Contains("flag", StringComparison.OrdinalIgnoreCase)
            || type.Contains("block", StringComparison.OrdinalIgnoreCase);
    }
}

