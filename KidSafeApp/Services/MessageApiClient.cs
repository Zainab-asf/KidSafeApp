using KidSafeApp.Shared.DTOs.Chat;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class MessageApiClient
{
    private readonly HttpClient _http;

    public MessageApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task SendMessageAsync(MessageSendDto dto, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync("api/messages", dto, cancellationToken);

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(int otherUserId, CancellationToken cancellationToken = default)
        => await _http.GetFromJsonAsync<List<MessageDto>>($"api/messages/{otherUserId}", cancellationToken) ?? new List<MessageDto>();
}

