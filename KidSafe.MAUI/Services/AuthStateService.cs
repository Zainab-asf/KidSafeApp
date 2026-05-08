using System.Text.Json;
using KidSafe.Shared.DTOs;

namespace KidSafe.MAUI.Services;

/// <summary>
/// MAUI version — uses Preferences instead of IJSRuntime localStorage.
/// </summary>
public class AuthStateService
{
    private const string Key = "kidsafe_user";

    public AuthResponse? CurrentUser { get; private set; }
    public event Action? OnChange;

    public Task InitAsync()
    {
        var json = Preferences.Default.Get<string?>(Key, null);
        if (!string.IsNullOrEmpty(json))
            CurrentUser = JsonSerializer.Deserialize<AuthResponse>(json);
        return Task.CompletedTask;
    }

    public Task SetUserAsync(AuthResponse user)
    {
        CurrentUser = user;
        Preferences.Default.Set(Key, JsonSerializer.Serialize(user));
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        Preferences.Default.Remove(Key);
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public bool IsAuthenticated   => CurrentUser != null && CurrentUser.Token is not ("pending" or "disabled");
    public bool IsChild           => CurrentUser?.Role == "Child";
    public bool IsParentOrTeacher => CurrentUser?.Role is "Parent" or "Teacher";
    public bool IsAdmin           => CurrentUser?.Role == "Admin";
}
