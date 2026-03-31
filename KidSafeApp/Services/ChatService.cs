using System.Collections.ObjectModel;
using KidSafeApp.Models;

namespace KidSafeApp.Services;

/// <summary>
/// Frontend chat service for the MAUI app.
/// Later this will call the backend (SignalR / HTTP) but for now
/// it just keeps messages in memory.
/// </summary>
public sealed class ChatService
{
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public Task InitializeAsync()
    {
        // TODO: call backend (e.g., GET /api/messages/recent) and fill Messages.
        return Task.CompletedTask;
    }

    public Task SendAsync(ChatMessage message)
    {
        // TODO: call backend (SignalR or HTTP POST), then add returned message.
        Messages.Add(message);
        return Task.CompletedTask;
    }
}    