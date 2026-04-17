using System.Collections.ObjectModel;
using KidSafeApp.Models;
using KidSafeApp.Helpers;
using KidSafeApp.Shared.DTOs.Chat;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

/// <summary>
/// Frontend chat service for the MAUI app.
/// Later this will call the backend (SignalR / HTTP) but for now
/// it just keeps messages in memory.
/// </summary>
public sealed class ChatService
{
    private readonly HttpClient _httpClient;

    public ChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task SendAsync(ChatMessage message)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MessageDto>?> GetMessagesAsync(int otherUserId, CancellationToken cancellationToken = default)
    {
        return _httpClient.GetFromJsonAsync<IReadOnlyList<MessageDto>>($"api/messages/{otherUserId}", JsonConverter.JsonSerializerOptions, cancellationToken);
    }

    public async Task<bool> SendMessageAsync(MessageSendDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/messages", dto, JsonConverter.JsonSerializerOptions, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}    