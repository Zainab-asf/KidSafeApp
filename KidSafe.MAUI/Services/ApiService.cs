using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KidSafe.Shared.DTOs;

namespace KidSafe.MAUI.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly AuthStateService _auth;

    public ApiService(HttpClient http, AuthStateService auth)
    {
        _http = http;
        _auth = auth;
    }

    private void SetAuthHeader()
    {
        if (_auth.CurrentUser != null)
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.CurrentUser.Token);
    }

    // ── Auth ─────────────────────────────────────────────────────────

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest req)
    {
        var resp = await _http.PostAsJsonAsync("auth/register", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<AuthResponse>() : null;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest req)
    {
        var resp = await _http.PostAsJsonAsync("auth/login", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<AuthResponse>() : null;
    }

    // ── Messages ─────────────────────────────────────────────────────

    public async Task<MessageResult?> SendMessageAsync(SendMessageRequest req)
    {
        SetAuthHeader();
        var resp = await _http.PostAsJsonAsync("messages/send", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<MessageResult>() : null;
    }

    public async Task<List<FlaggedMessageItem>> GetFlaggedMessagesAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<FlaggedMessageItem>>("messages/flagged")
               ?? new List<FlaggedMessageItem>();
    }

    // ── Dashboard ────────────────────────────────────────────────────

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<DashboardStats>("dashboard/stats");
    }

    // ── FCM ──────────────────────────────────────────────────────────

    public async Task RegisterFcmTokenAsync(string token)
    {
        SetAuthHeader();
        await _http.PostAsJsonAsync("rewards/fcm-token", new { Token = token });
    }

    // ── Generic helpers ───────────────────────────────────────────────

    public async Task<T?> GetAsync<T>(string url)
    {
        SetAuthHeader();
        try { return await _http.GetFromJsonAsync<T>(url); }
        catch { return default; }
    }

    public async Task<bool> PostAsync(string url, object body)
    {
        SetAuthHeader();
        var resp = await _http.PostAsJsonAsync(url, body);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> PatchAsync(string url, object body)
    {
        SetAuthHeader();
        var json    = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp    = await _http.PatchAsync(url, content);
        return resp.IsSuccessStatusCode;
    }

    // ── Reports / Complaints ─────────────────────────────────────────

    public async Task<bool> ReportAbuseAsync(string reason, int? flaggedMessageId = null)
    {
        SetAuthHeader();
        var resp = await _http.PostAsJsonAsync("reports/abuse",
            new { Reason = reason, FlaggedMessageId = flaggedMessageId });
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> FileComplaintAsync(string description, int? linkedReportId = null)
    {
        SetAuthHeader();
        var resp = await _http.PostAsJsonAsync("reports/complaint",
            new { Description = description, LinkedReportId = linkedReportId });
        return resp.IsSuccessStatusCode;
    }
}
