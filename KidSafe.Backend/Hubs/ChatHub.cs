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
    Task ReceiveClassMessage(int classId, int senderId, string senderName, string senderEmoji,
                             string content, string label, double score, DateTime timestamp);
    Task UserTyping(string userId, string userName);
    Task ClassUserTyping(int classId, string userName);
    Task FlaggedMessageAlert(int senderId, string senderName, string maskedMessage, string label, double score);
    Task ConnectionAck(string connectionId, string userId);
    Task UserStatusChanged(string userId, string displayName, bool online);
    Task NotificationReceived(string title, string body, string type);
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

    /// <summary>Parents, teachers, and admins call this to receive dashboard alerts.</summary>
    public async Task JoinParentRoom()
    {
        var role = Role();
        if (role is "Parent" or "Teacher" or "Admin")
            await Groups.AddToGroupAsync(Context.ConnectionId, "parents");
    }

    public async Task LeaveParentRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "parents");
    }

    /// <summary>Join a class group room.</summary>
    public async Task JoinClass(int classId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ClassGroup(classId));
    }

    public async Task LeaveClass(int classId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ClassGroup(classId));
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

    /// <summary>Typing indicator for class group chat.</summary>
    public async Task SendClassTypingIndicator(int classId)
    {
        var name = DisplayName();
        if (name == null) return;
        await Clients.OthersInGroup(ClassGroup(classId))
                     .ClassUserTyping(classId, name);
    }

    // ── helpers ───────────────────────────────────────────────

    private string? UserId()      => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string? DisplayName() => Context.User?.FindFirst("displayName")?.Value;
    private string? Role()        => Context.User?.FindFirst(ClaimTypes.Role)?.Value;
    private static string UserGroup(string userId) => $"user_{userId}";
    private static string ClassGroup(int classId)  => $"class_{classId}";
}
