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

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null)
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
