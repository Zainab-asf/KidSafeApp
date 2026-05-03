using Microsoft.AspNetCore.SignalR.Client;
using KidSafeApp.Shared.Chat;
using KidSafeApp.Shared.DTOs.Auth;
using KidSafeApp.Shared.DTOs.Chat;
using KidSafeApp.StateManagement;

namespace KidSafeApp.Services;

public sealed class HubConnectionService : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _authenticationState;
    private HubConnection? _hubConnection;
    private bool _isConnecting = false;

    public HubConnection? Connection => _hubConnection;
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Func<UserDto, Task>? OnUserConnected;
    public event Func<IEnumerable<UserDto>, Task>? OnOnlineUsersList;
    public event Func<int, Task>? OnUserIsOnline;
    public event Func<MessageDto, Task>? OnMessageReceived;
    public event Func<int, Task>? OnRosterUpdated;

    public HubConnectionService(HttpClient httpClient, AuthenticationState authenticationState)
    {
        _httpClient = httpClient;
        _authenticationState = authenticationState;
    }

    /// <summary>
    /// Connects to the hub AND marks the current user as online in one call.
    /// Call this once when entering the chat page.
    /// </summary>
    public async Task InitializeAsync()
    {
        await ConnectAsync();

        if (_authenticationState?.User is not null && _authenticationState.User.Id > 0)
        {
            await SetUserOnlineAsync(_authenticationState.User);
        }
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null && _hubConnection.State != HubConnectionState.Disconnected)
        {
            return;
        }

        if (_isConnecting)
        {
            return;
        }

        _isConnecting = true;
        try
        {
            // Dispose old disconnected connection if any
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }

            _hubConnection = ConfigureConnection();
            RegisterEventHandlers();
            await _hubConnection.StartAsync();
        }
        finally
        {
            _isConnecting = false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async Task SetUserOnlineAsync(UserDto user)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync(nameof(IChatHubServer.SetUserOnline), user);
        }
    }

    /// <summary>
    /// Sends a message through SignalR hub directly (real-time delivery).
    /// </summary>
    public async Task SendMessageViaHubAsync(int toUserId, string content)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("SendMessage", toUserId, content);
        }
    }

    private HubConnection ConfigureConnection()
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("HttpClient.BaseAddress is not configured.");
        }

        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/kidsafeapp");
        return new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
                options.AccessTokenProvider = () =>
                    Task.FromResult<string?>(_authenticationState?.Token ?? null))
            .WithAutomaticReconnect()
            .Build();
    }

    private void RegisterEventHandlers()
    {
        if (_hubConnection is null)
        {
            return;
        }

        _hubConnection.On<UserDto>(nameof(IChatHubClient.UserConnected),
            user => OnUserConnected?.Invoke(user) ?? Task.CompletedTask);

        _hubConnection.On<IEnumerable<UserDto>>(nameof(IChatHubClient.OnlineUsersList),
            users => OnOnlineUsersList?.Invoke(users) ?? Task.CompletedTask);

        _hubConnection.On<int>(nameof(IChatHubClient.UserIsOnline),
            userId => OnUserIsOnline?.Invoke(userId) ?? Task.CompletedTask);

        _hubConnection.On<MessageDto>(nameof(IChatHubClient.MessageRecieved),
            message => OnMessageReceived?.Invoke(message) ?? Task.CompletedTask);

        _hubConnection.On<int>(nameof(IChatHubClient.RosterUpdated),
            classRoomId => OnRosterUpdated?.Invoke(classRoomId) ?? Task.CompletedTask);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
