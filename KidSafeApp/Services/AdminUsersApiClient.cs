using KidSafeApp.Shared.DTOs.Admin;
using KidSafeApp.Shared.DTOs.Common;
using KidSafeApp.StateManagement;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class AdminUsersApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _authenticationState;

    public AdminUsersApiClient(HttpClient httpClient, AuthenticationState authenticationState)
    {
        _httpClient = httpClient;
        _authenticationState = authenticationState;
    }

    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(AdminUsersQueryDto query, CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var queryString =
            $"api/admin/users/paged?pageNumber={query.PageNumber}&pageSize={query.PageSize}" +
            $"&search={Uri.EscapeDataString(query.Search ?? string.Empty)}" +
            $"&role={Uri.EscapeDataString(query.Role ?? string.Empty)}" +
            $"&isActive={query.IsActive?.ToString() ?? string.Empty}" +
            $"&isApproved={query.IsApproved?.ToString() ?? string.Empty}";

        return await _httpClient.GetFromJsonAsync<PagedResultDto<AdminUserDto>>(queryString, cancellationToken)
            ?? new PagedResultDto<AdminUserDto> { Items = Array.Empty<AdminUserDto>(), PageNumber = 1, PageSize = query.PageSize, TotalCount = 0 };
    }

    public async Task<AdminUserDto> CreateUserAsync(AdminCreateUserDto dto, CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var response = await _httpClient.PostAsJsonAsync("api/admin/users", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return await response.Content.ReadFromJsonAsync<AdminUserDto>(cancellationToken)
            ?? throw new InvalidOperationException("Unable to parse created user.");
    }

    public async Task<IReadOnlyList<AdminClassRoomDto>> GetClassRoomsAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var result = await _httpClient.GetFromJsonAsync<List<AdminClassRoomDto>>("api/admin/classrooms", cancellationToken);
        return result ?? new List<AdminClassRoomDto>();
    }

    public async Task<IReadOnlyList<AdminCourseLiteDto>> GetCoursesAsync(CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var result = await _httpClient.GetFromJsonAsync<List<AdminCourseLiteDto>>("api/admin/courses", cancellationToken);
        return result ?? new List<AdminCourseLiteDto>();
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        ApplyBearerToken();
        var response = await _httpClient.DeleteAsync($"api/admin/users/{userId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(cancellationToken));
        }
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
