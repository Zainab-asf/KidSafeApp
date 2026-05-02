using KidSafeApp.Shared.DTOs.Auth;
using KidSafeApp.Shared.DTOs.Chat;
using Microsoft.AspNetCore.SignalR.Client;

namespace KidSafeApp.Services;

public sealed class HubConnectionService : IAsyncDisposable
{
    private readonly HttpClient _http;
    private HubConnection? _connection;

    public HubConnectionService(HttpClient http)
    {
        _http = http;
    }

    public event Func<UserDto, Task>? OnUserConnected;
    public event Func<IEnumerable<UserDto>, Task>? OnOnlineUsersList;
    public event Func<int, Task>? OnUserIsOnline;
    public event Func<MessageDto, Task>? OnMessageReceived;
    public event Func<int, Task>? OnRosterUpdated;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                return;
            }
            await _connection.StartAsync(cancellationToken);
            return;
        }

        var baseUri = _http.BaseAddress ?? new Uri("http://localhost:5065/");
        var hubUrl = new Uri(baseUri, "chathub").ToString();

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<UserDto>("UserConnected", user => OnUserConnected?.Invoke(user) ?? Task.CompletedTask);
        _connection.On<IEnumerable<UserDto>>("OnlineUsersList", users => OnOnlineUsersList?.Invoke(users) ?? Task.CompletedTask);
        _connection.On<int>("UserIsOnline", userId => OnUserIsOnline?.Invoke(userId) ?? Task.CompletedTask);
        _connection.On<MessageDto>("MessageRecieved", msg => OnMessageReceived?.Invoke(msg) ?? Task.CompletedTask);
        _connection.On<int>("RosterUpdated", classRoomId => OnRosterUpdated?.Invoke(classRoomId) ?? Task.CompletedTask);

        await _connection.StartAsync(cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
        => _connection is null ? Task.CompletedTask : _connection.StopAsync(cancellationToken);

    public Task SetUserOnlineAsync(UserDto user, CancellationToken cancellationToken = default)
        => _connection is null ? Task.CompletedTask : _connection.InvokeAsync("SetUserOnline", user, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}

