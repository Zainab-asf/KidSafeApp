using System.Net.Http.Headers;
using KidSafeApp.Helpers;
using KidSafeApp.Services;
using KidSafeApp.Shared.DTOs.Chat;
using System.Net.Http.Json;

namespace KidSafeApp.Services;

public sealed class MessageApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationService _authService;
    private readonly ErrorService _errorService;

    public MessageApiClient(
        HttpClient httpClient,
        AuthenticationService authService,
        ErrorService errorService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _errorService = errorService;
    }

    /// <summary>
    /// Sends a message to another user.
    /// </summary>
    public async Task<MessageDto?> SendMessageAsync(int toUserId, string content, CancellationToken cancellationToken = default)
    {
        if (toUserId <= 0 || string.IsNullOrWhiteSpace(content))
        {
            _errorService.ShowError("Invalid recipient or message content.");
            throw new ArgumentException("Invalid parameters.");
        }

        try
        {
            ApplyBearerToken();

            var messageDto = new MessageSendDto(toUserId, content);
            var response = await _httpClient.PostAsJsonAsync(
                "api/messages",
                messageDto,
                JsonHelper.DefaultOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _errorService.ShowError(errorContent ?? "Failed to send message.");
                throw new InvalidOperationException(
                    $"Failed to send message: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<MessageDto>(
                JsonHelper.DefaultOptions, cancellationToken);
            
            return result;
        }
        catch (HttpRequestException)
        {
            _errorService.ShowError("Connection error. Please check your network.", "Network Error");
            throw;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            _errorService.ShowError("An unexpected error occurred.", "Error");
            throw;
        }
    }

    /// <summary>
    /// Retrieves message history with another user.
    /// </summary>
    public async Task<IEnumerable<MessageDto>?> GetMessagesAsync(
        int otherUserId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (otherUserId <= 0)
        {
            _errorService.ShowError("Invalid user ID.");
            throw new ArgumentException("Invalid user ID.");
        }

        try
        {
            ApplyBearerToken();

            var url = $"api/messages/{otherUserId}?pageNumber={pageNumber}&pageSize={pageSize}";
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<MessageDto>>(
                url,
                JsonHelper.DefaultOptions,
                cancellationToken);

            return result ?? Enumerable.Empty<MessageDto>();
        }
        catch (HttpRequestException)
        {
            _errorService.ShowError("Failed to load messages. Please check your connection.", "Connection Error");
            throw;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _errorService.ShowError("Failed to load messages.");
            throw;
        }
    }

    /// <summary>
    /// Marks a message as read.
    /// </summary>
    public async Task MarkAsReadAsync(int messageId, CancellationToken cancellationToken = default)
    {
        if (messageId <= 0)
        {
            throw new ArgumentException("Invalid message ID.");
        }

        try
        {
            ApplyBearerToken();

            var response = await _httpClient.PutAsync(
                $"api/messages/{messageId}/read",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Silently fail - not critical
                throw new InvalidOperationException(
                    $"Failed to mark message as read: {response.StatusCode}");
            }
        }
        catch (HttpRequestException)
        {
            // Network error - don't show to user for this non-critical operation
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves chat previews for the current user.
    /// </summary>
    public async Task<IEnumerable<ChatPreviewDto>?> GetChatsPreviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            ApplyBearerToken();

            var result = await _httpClient.GetFromJsonAsync<IEnumerable<ChatPreviewDto>>(
                "api/messages/chats/preview",
                JsonHelper.DefaultOptions,
                cancellationToken);

            return result ?? Enumerable.Empty<ChatPreviewDto>();
        }
        catch (HttpRequestException)
        {
            _errorService.ShowError("Failed to load chats.", "Connection Error");
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void ApplyBearerToken()
    {
        if (!string.IsNullOrWhiteSpace(_authService.CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
        }
    }
}
