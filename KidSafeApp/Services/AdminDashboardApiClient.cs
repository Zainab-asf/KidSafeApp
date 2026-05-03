using KidSafeApp.Shared.DTOs.Admin;
using KidSafeApp.StateManagement;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class AdminDashboardApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _authenticationState;

    public AdminDashboardApiClient(HttpClient httpClient, AuthenticationState authenticationState)
    {
        _httpClient = httpClient;
        _authenticationState = authenticationState;
    }

    public async Task<AdminDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<AdminDashboardSummaryDto>("api/admin/dashboard/summary", cancellationToken)
            ?? new AdminDashboardSummaryDto();
    }

    public async Task<AdminChildChatOverviewDto> GetChildChatAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<AdminChildChatOverviewDto>("api/admin/dashboard/child-chat", cancellationToken)
            ?? new AdminChildChatOverviewDto();
    }

    public async Task<List<AdminChatLineDto>> GetChildChatMessagesAsync(int studentId, CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var url = $"api/admin/dashboard/child-chat/{studentId}/messages";
        return await _httpClient.GetFromJsonAsync<List<AdminChatLineDto>>(url, cancellationToken)
            ?? new List<AdminChatLineDto>();
    }

    public async Task<AdminParentPortalSummaryDto> GetParentPortalAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<AdminParentPortalSummaryDto>("api/admin/dashboard/parent-portal", cancellationToken)
            ?? new AdminParentPortalSummaryDto();
    }

    public async Task<AdminTeacherModuleSummaryDto> GetTeacherModuleAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<AdminTeacherModuleSummaryDto>("api/admin/dashboard/teacher-module", cancellationToken)
            ?? new AdminTeacherModuleSummaryDto();
    }

    public async Task<AdminNotificationFeedDto> GetNotificationsAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<AdminNotificationFeedDto>("api/admin/dashboard/notifications", cancellationToken)
            ?? new AdminNotificationFeedDto();
    }

    public async Task<List<AdminHelpItemDto>> GetHelpItemsAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<List<AdminHelpItemDto>>("api/admin/dashboard/help", cancellationToken)
            ?? new List<AdminHelpItemDto>();
    }

    public async Task<List<AdminSupportTicketDto>> GetSupportTicketsAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        return await _httpClient.GetFromJsonAsync<List<AdminSupportTicketDto>>("api/admin/dashboard/support", cancellationToken)
            ?? new List<AdminSupportTicketDto>();
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
