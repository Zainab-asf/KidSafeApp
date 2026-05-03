using KidSafeApp.Shared.DTOs.Admin;
using KidSafeApp.Shared.DTOs.Common;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class AdminUsersApiClient
{
    private readonly HttpClient _http;

    public AdminUsersApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(AdminUsersQueryDto query, CancellationToken cancellationToken = default)
    {
        var url =
            $"api/admin/users/paged" +
            $"?pageNumber={query.PageNumber}" +
            $"&pageSize={query.PageSize}" +
            $"&search={Uri.EscapeDataString(query.Search ?? string.Empty)}" +
            $"&role={Uri.EscapeDataString(query.Role ?? string.Empty)}" +
            $"&isActive={query.IsActive?.ToString() ?? string.Empty}" +
            $"&isApproved={query.IsApproved?.ToString() ?? string.Empty}";

        return await _http.GetFromJsonAsync<PagedResultDto<AdminUserDto>>(url, cancellationToken)
            ?? new PagedResultDto<AdminUserDto>();
    }

    public Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        => _http.DeleteAsync($"api/admin/users/{id}", cancellationToken);

    public async Task<IReadOnlyList<AdminClassRoomDto>> GetClassRoomsAsync(CancellationToken cancellationToken = default)
        => await _http.GetFromJsonAsync<List<AdminClassRoomDto>>("api/admin/classrooms", cancellationToken) ?? new List<AdminClassRoomDto>();

    public async Task<IReadOnlyList<AdminCourseLiteDto>> GetCoursesAsync(CancellationToken cancellationToken = default)
        => await _http.GetFromJsonAsync<List<AdminCourseLiteDto>>("api/admin/courses", cancellationToken) ?? new List<AdminCourseLiteDto>();
}

