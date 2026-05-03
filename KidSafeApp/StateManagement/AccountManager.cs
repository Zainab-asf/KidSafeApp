using KidSafeApp.Helpers;
using System.Text.Json;
using Microsoft.JSInterop;

namespace KidSafeApp.StateManagement;

/// <summary>
/// Manages multiple user accounts for account switching within the same app instance
/// </summary>
public class AccountManager
{
    private const string ACCOUNTS_STORAGE_KEY = "kidsafe_accounts";
    private const string CURRENT_ACCOUNT_KEY = "kidsafe_current_account";
    private readonly IJSRuntime _jsRuntime;

    public Dictionary<string, AuthenticationState> StoredAccounts { get; private set; } = new();
    public string? CurrentAccountId { get; private set; }

    public event Action? OnAccountsChanged;
    public event Action? OnAccountSwitched;

    public AccountManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initialize and load stored accounts from localStorage
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var accountsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ACCOUNTS_STORAGE_KEY);
            if (!string.IsNullOrEmpty(accountsJson))
            {
                StoredAccounts = JsonSerializer.Deserialize<Dictionary<string, AuthenticationState>>(
                    accountsJson, JsonHelper.DefaultOptions) ?? new();
            }

            CurrentAccountId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CURRENT_ACCOUNT_KEY);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading accounts: {ex.Message}");
        }
    }

    /// <summary>
    /// Save current authentication state as an account
    /// </summary>
    public async Task SaveAccountAsync(AuthenticationState authState, string accountLabel)
    {
        var accountId = Guid.NewGuid().ToString();
        StoredAccounts[accountId] = authState;

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", 
                ACCOUNTS_STORAGE_KEY, 
                JsonSerializer.Serialize(StoredAccounts, JsonHelper.DefaultOptions));

        OnAccountsChanged?.Invoke();
    }

    /// <summary>
    /// Switch to a different stored account
    /// </summary>
    public async Task SwitchAccountAsync(string accountId)
    {
        if (StoredAccounts.TryGetValue(accountId, out var authState))
        {
            CurrentAccountId = accountId;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CURRENT_ACCOUNT_KEY, accountId);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", 
                "auth", 
                JsonSerializer.Serialize(authState, JsonHelper.DefaultOptions));

            OnAccountSwitched?.Invoke();
        }
    }

    /// <summary>
    /// Get the currently active account
    /// </summary>
    public AuthenticationState? GetCurrentAccount()
    {
        if (CurrentAccountId != null && StoredAccounts.TryGetValue(CurrentAccountId, out var account))
        {
            return account;
        }
        return null;
    }

    /// <summary>
    /// Remove a stored account
    /// </summary>
    public async Task RemoveAccountAsync(string accountId)
    {
        StoredAccounts.Remove(accountId);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", 
            ACCOUNTS_STORAGE_KEY, 
            JsonSerializer.Serialize(StoredAccounts, JsonHelper.DefaultOptions));

        if (CurrentAccountId == accountId)
        {
            CurrentAccountId = StoredAccounts.Keys.FirstOrDefault();
            if (CurrentAccountId != null)
            {
                await SwitchAccountAsync(CurrentAccountId);
            }
        }

        OnAccountsChanged?.Invoke();
    }

    /// <summary>
    /// List all stored accounts
    /// </summary>
    public IEnumerable<(string Id, string UserName)> GetStoredAccounts()
    {
        return StoredAccounts.Select(kvp => (kvp.Key, kvp.Value.User?.Name ?? "Unknown"));
    }

    /// <summary>
    /// Check if an account is currently logged in
    /// </summary>
    public bool IsAccountStored(string userName)
    {
        return StoredAccounts.Values.Any(a => a.User?.Name == userName);
    }
}
