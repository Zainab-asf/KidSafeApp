using KidSafeApp.Backend.Services.Chat;
using KidSafeApp.Shared.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KidSafeApp.Backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatHubClient>, IChatHubServer
    {
        private static readonly IDictionary<int, UserDto> _onlineUsers = new Dictionary<int, UserDto>();
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("User connected to chat hub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove user from online list and notify others they went offline
            if (TryGetCurrentUserId(out var parsedUserId) && _onlineUsers.ContainsKey(parsedUserId))
            {
                _onlineUsers.Remove(parsedUserId);
                await Clients.Others.UserIsOnline(parsedUserId); // userId with IsOnline=false indicates offline
                _logger.LogInformation("User {UserId} disconnected from chat hub", parsedUserId);
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task SetUserOnline(UserDto user)
        {
            try
            {
                await Clients.Caller.OnlineUsersList(_onlineUsers.Values);
                if (!_onlineUsers.ContainsKey(user.Id))
                {
                    _onlineUsers.Add(user.Id, user);
                    await Clients.Others.UserConnected(user);
                }
                _logger.LogInformation("User {UserId} set online", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user {UserId} online", user.Id);
                throw;
            }
        }

        public async Task SendMessage(int toUserId, string content)
        {
            try
            {
                if (!TryGetCurrentUserId(out var parsedFromUserId))
                {
                    throw new UnauthorizedAccessException("User ID not found in token");
                }

                var messageDto = await _messageService.SendMessageAsync(
                    parsedFromUserId,
                    toUserId,
                    content,
                    CancellationToken.None);

                // Send to recipient if they're online (include flagged info)
                await Clients.User(toUserId.ToString()).MessageRecieved(messageDto);

                // Notify sender if message was flagged
                if (messageDto.IsFlagged)
                {
                    await Clients.Caller.MessageFlagged(new { messageId = messageDto.Id, reason = messageDto.FlagReason });
                }
                
                _logger.LogInformation(
                    "Message sent from user {FromUserId} to user {ToUserId}",
                    parsedFromUserId, toUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to user {ToUserId}", toUserId);
                throw;
            }
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;
            var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;
            return int.TryParse(value, out userId) && userId > 0;
        }
    }
}
