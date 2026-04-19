using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Controllers.Common;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Backend.Hubs;
using KidSafeApp.Shared.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Controllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : BaseController
    {
        private readonly DataContext _chatContext;
        private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;

        public MessagesController(DataContext chatContext, IHubContext<ChatHub, IChatHubClient> hubContext)
        {
            _chatContext = chatContext;
            _hubContext = hubContext;
        }

        // /api/messages
        [HttpPost("")]
        public async Task<IActionResult> SendMessage(MessageSendDto messageDto, CancellationToken cancellationToken)
        {
            if (messageDto.ToUserId <= 0 || string.IsNullOrWhiteSpace(messageDto.Message))
                return BadRequest();

            if (!await CanExchangeMessagesAsync(UserId, messageDto.ToUserId, cancellationToken))
            {
                return Forbid();
            }

            var message = new Message
            {
                FromId = base.UserId,
                ToId = messageDto.ToUserId,
                Content = messageDto.Message,
                SentOn = DateTime.Now
            };
            await _chatContext.Messages.AddAsync(message, cancellationToken);
            if(await _chatContext.SaveChangesAsync(cancellationToken) > 0)
            {
                var responseMessageDto = new MessageDto(message.ToId, message.FromId, message.Content, message.SentOn);
                await _hubContext.Clients.User(messageDto.ToUserId.ToString())
                            .MessageRecieved(responseMessageDto);
                return Ok();
            }
            else
            {
                return StatusCode(500, "Unable to send message");
            }
        }

        [HttpGet("{otherUserId:int}")]
        public async Task<IEnumerable<MessageDto>> GetMessages(int otherUserId, CancellationToken cancellationToken)
        {
            if (!await CanExchangeMessagesAsync(UserId, otherUserId, cancellationToken))
            {
                return Enumerable.Empty<MessageDto>();
            }

            var messages = await _chatContext.Messages
                            .AsNoTracking()
                            .Where(m =>
                                (m.FromId == otherUserId && m.ToId == UserId)
                                || (m.ToId == otherUserId && m.FromId == UserId)
                            )
                            .Select(m=> new MessageDto(m.ToId, m.FromId, m.Content, m.SentOn))
                            .ToListAsync(cancellationToken);

            return messages;
        }

        private async Task<bool> CanExchangeMessagesAsync(int currentUserId, int otherUserId, CancellationToken cancellationToken)
        {
            if (currentUserId == otherUserId)
            {
                return false;
            }

            var currentUser = await _chatContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId && u.IsActive && u.IsApproved, cancellationToken);

            if (currentUser is null)
            {
                return false;
            }

            if (!string.Equals(currentUser.Role, "Child", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var otherUser = await _chatContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == otherUserId && u.IsActive && u.IsApproved, cancellationToken);

            if (otherUser is null || !string.Equals(otherUser.Role, "Child", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var currentClassIds = await _chatContext.ClassRoomStudents
                .AsNoTracking()
                .Where(cs => cs.StudentId == currentUserId)
                .Select(cs => cs.ClassRoomId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (currentClassIds.Count == 0)
            {
                return false;
            }

            return await _chatContext.ClassRoomStudents
                .AsNoTracking()
                .AnyAsync(cs => currentClassIds.Contains(cs.ClassRoomId) && cs.StudentId == otherUserId, cancellationToken);
        }
    }
}
