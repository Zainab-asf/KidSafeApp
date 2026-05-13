using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KidSafe.Shared.DTOs;

namespace KidSafe.MAUI.Services;

/// <summary>
/// Typed HTTP client for the KidSafe backend.
/// Auth header is injected by <see cref="AuthDelegatingHandler"/> — no manual header calls needed.
/// </summary>
public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http) => _http = http;

    public string BaseUrl => _http.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5000";

    // ── Auth ──────────────────────────────────────────────────────────

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

    // ── Messages ──────────────────────────────────────────────────────

    public async Task<MessageResult?> SendMessageAsync(SendMessageRequest req)
    {
        var resp = await _http.PostAsJsonAsync("messages/send", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<MessageResult>() : null;
    }

    public async Task<List<FlaggedMessageItem>> GetFlaggedMessagesAsync()
        => await _http.GetFromJsonAsync<List<FlaggedMessageItem>>("messages/flagged")
           ?? new List<FlaggedMessageItem>();

    // ── Dashboard ─────────────────────────────────────────────────────

    public async Task<DashboardStats?> GetDashboardStatsAsync()
        => await _http.GetFromJsonAsync<DashboardStats>("dashboard/stats");

    // ── FCM ───────────────────────────────────────────────────────────

    public async Task RegisterFcmTokenAsync(string token)
        => await _http.PostAsJsonAsync("rewards/fcm-token", new { Token = token });

    // ── Generic helpers ───────────────────────────────────────────────

    public async Task<T?> GetAsync<T>(string url)
    {
        try   { return await _http.GetFromJsonAsync<T>(url); }
        catch { return default; }
    }

    public async Task<bool> PostAsync(string url, object body)
    {
        var resp = await _http.PostAsJsonAsync(url, body);
        return resp.IsSuccessStatusCode;
    }

    public async Task<T?> PostRawAsync<T>(string url, object body)
    {
        var resp = await _http.PostAsJsonAsync(url, body);
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    public async Task<bool> DeleteAsync(string url)
    {
        var resp = await _http.DeleteAsync(url);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> PatchAsync(string url, object body)
    {
        var json    = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp    = await _http.PatchAsync(url, content);
        return resp.IsSuccessStatusCode;
    }

    public async Task<T?> PatchRawAsync<T>(string url, object body)
    {
        var json    = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp    = await _http.PatchAsync(url, content);
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>Returns (success, errorMessage). On failure, tries to extract message from JSON body.</summary>
    public async Task<(bool Ok, string Error)> PostWithErrorAsync(string url, object body)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(url, body);
            if (resp.IsSuccessStatusCode) return (true, "");
            var raw = await resp.Content.ReadAsStringAsync();
            try
            {
                var el = JsonSerializer.Deserialize<JsonElement>(raw);
                if (el.TryGetProperty("message", out var m)) return (false, m.GetString() ?? raw);
                if (el.TryGetProperty("title",   out var t)) return (false, t.GetString() ?? raw);
            }
            catch { }
            return (false, string.IsNullOrWhiteSpace(raw) ? "Request failed." : raw.Trim('"'));
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // ── Reports / Complaints ─────────────────────────────────────────

    public async Task<bool> ReportAbuseAsync(string reason, int? flaggedMessageId = null)
    {
        var resp = await _http.PostAsJsonAsync("reports/abuse",
            new { Reason = reason, FlaggedMessageId = flaggedMessageId });
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> FileComplaintAsync(string description, int? linkedReportId = null)
    {
        var resp = await _http.PostAsJsonAsync("reports/complaint",
            new { Description = description, LinkedReportId = linkedReportId });
        return resp.IsSuccessStatusCode;
    }
}
