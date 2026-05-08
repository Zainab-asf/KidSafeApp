using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KidSafe.Backend.Hubs;

/// <summary>
/// Strongly-typed contract pushed from server → client.
/// </summary>
public interface IChatClient
{
    Task ReceiveMessage(int senderId, string senderName, string message, string label);
    Task UserTyping(string userId, string userName);
    Task FlaggedMessageAlert(int senderId, string senderName, string maskedMessage, string label, double score);
    Task ConnectionAck(string connectionId, string userId);
    Task UserStatusChanged(string userId, string displayName, bool online);
}

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    // ── lifecycle ─────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId      = UserId();
        var displayName = DisplayName();

        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            // Notify the connecting client its server-assigned connectionId
            await Clients.Caller.ConnectionAck(Context.ConnectionId, userId);

            // Broadcast presence to everyone in that user's group
            await Clients.Others.UserStatusChanged(userId, displayName ?? userId, true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId      = UserId();
        var displayName = DisplayName();

        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId));
            await Clients.Others.UserStatusChanged(userId, displayName ?? userId, false);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ── client-callable methods ───────────────────────────────

    /// <summary>Parents and teachers call this to receive dashboard alerts.</summary>
    public async Task JoinParentRoom()
    {
        var role = Role();
        if (role is "Parent" or "Teacher")
            await Groups.AddToGroupAsync(Context.ConnectionId, "parents");
    }

    public async Task LeaveParentRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "parents");
    }

    /// <summary>Child sends typing indicator to a specific receiver.</summary>
    public async Task SendTypingIndicator(int receiverId)
    {
        var userId = UserId();
        var name   = DisplayName();
        if (userId == null) return;

        await Clients.Group(UserGroup(receiverId.ToString()))
                     .UserTyping(userId, name ?? "Someone");
    }

    // ── helpers ───────────────────────────────────────────────

    private string? UserId()      => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string? DisplayName() => Context.User?.FindFirst("displayName")?.Value;
    private string? Role()        => Context.User?.FindFirst(ClaimTypes.Role)?.Value;
    private static string UserGroup(string userId) => $"user_{userId}";
}
