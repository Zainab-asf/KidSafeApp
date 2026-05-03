using Microsoft.AspNetCore.SignalR.Client;
using KidSafeApp.Shared.Chat;
using KidSafeApp.Shared.DTOs.Auth;
using KidSafeApp.Shared.DTOs.Chat;

namespace KidSafeApp.Services.Interfaces;

/// <summary>
/// Provides SignalR hub connection functionality for real-time communication.
/// </summary>
public interface IHubConnectionService : IAsyncDisposable
{
    /// <summary>
    /// Gets the underlying SignalR hub connection, or null if not connected.
    /// </summary>
    HubConnection? Connection { get; }

    /// <summary>
    /// Gets a value indicating whether the hub connection is established.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when a user connects to the hub.
    /// </summary>
    event Func<UserDto, Task>? OnUserConnected;

    /// <summary>
    /// Event raised when the list of online users is received.
    /// </summary>
    event Func<IEnumerable<UserDto>, Task>? OnOnlineUsersList;

    /// <summary>
    /// Event raised when a specific user comes online.
    /// </summary>
    event Func<int, Task>? OnUserIsOnline;

    /// <summary>
    /// Event raised when a message is received from the hub.
    /// </summary>
    event Func<MessageDto, Task>? OnMessageReceived;

    /// <summary>
    /// Event raised when the roster is updated.
    /// </summary>
    event Func<int, Task>? OnRosterUpdated;

    /// <summary>
    /// Establishes a connection to the SignalR hub.
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// Disconnects from the SignalR hub.
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Notifies the server that the user is online.
    /// </summary>
    /// <param name="user">The user DTO.</param>
    Task SetUserOnlineAsync(UserDto user);
}