using KidSafeApp.Backend.Controllers.Common;
using KidSafeApp.Backend.Services.Chat;
using KidSafeApp.Shared.DTOs.Chat;
using Microsoft.AspNetCore.Mvc;

namespace KidSafeApp.Backend.Controllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : BaseController
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a message from the current user to another user.
        /// </summary>
        [HttpPost("")]
        public async Task<IActionResult> SendMessage(MessageSendDto messageDto, CancellationToken cancellationToken)
        {
            if (messageDto.ToUserId <= 0 || string.IsNullOrWhiteSpace(messageDto.Message))
            {
                return BadRequest("Invalid recipient or message content.");
            }

            try
            {
                var message = await _messageService.SendMessageAsync(
                    UserId,
                    messageDto.ToUserId,
                    messageDto.Message,
                    cancellationToken);

                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} attempted to send message to unauthorized recipient {ToUserId}",
                    UserId, messageDto.ToUserId);
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid message parameters: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from {UserId} to {ToUserId}",
                    UserId, messageDto.ToUserId);
                return StatusCode(500, "An error occurred while sending the message.");
            }
        }

        /// <summary>
        /// Retrieves message history between the current user and another user.
        /// </summary>
        [HttpGet("{otherUserId:int}")]
        public async Task<IActionResult> GetMessages(int otherUserId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
        {
            if (otherUserId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            try
            {
                var messages = await _messageService.GetChatMessagesAsync(
                    UserId,
                    otherUserId,
                    pageNumber,
                    pageSize,
                    cancellationToken);

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for user {UserId} with {OtherUserId}",
                    UserId, otherUserId);
                return StatusCode(500, "An error occurred while retrieving messages.");
            }
        }

        /// <summary>
        /// Marks a message as read.
        /// </summary>
        [HttpPut("{messageId}/read")]
        public async Task<IActionResult> MarkAsRead(int messageId, CancellationToken cancellationToken)
        {
            if (messageId <= 0)
            {
                return BadRequest("Invalid message ID.");
            }

            try
            {
                await _messageService.MarkMessageAsReadAsync(messageId, cancellationToken);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
                return StatusCode(500, "An error occurred while marking the message as read.");
            }
        }

        /// <summary>
        /// Retrieves chat previews for the current user (most recent message per chat).
        /// </summary>
        [HttpGet("chats/preview")]
        public async Task<IActionResult> GetChatsPreviews(CancellationToken cancellationToken)
        {
            try
            {
                var chats = await _messageService.GetChatsPreviewAsync(UserId, cancellationToken);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat previews for user {UserId}", UserId);
                return StatusCode(500, "An error occurred while retrieving chats.");
            }
        }
    }
}
