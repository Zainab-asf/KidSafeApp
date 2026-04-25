using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Controllers.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KidSafeApp.Backend.Controllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly DataContext _dataContext;

        public UsersController(DataContext chatContext)
        {
            _dataContext = chatContext;

        }

        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetUsers(CancellationToken cancellationToken)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
            var allowedUserIds = await GetAllowedChatPartnerIdsAsync(UserId, role, cancellationToken);
            return await _dataContext.Users
                .AsNoTracking()
                .Where(u => allowedUserIds.Contains(u.Id) && u.IsActive && u.IsApproved)
                .Select(u => new UserDto(u.Id, u.Name, false, u.Role))
                .ToListAsync(cancellationToken);
        }

        [HttpGet("chats")]
        public async Task<IEnumerable<UserDto>> GetUserChats(CancellationToken cancellationToken)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
            IEnumerable<UserDto> chatUsers = new List<UserDto>();
            var uniqueUsers = await _dataContext.Messages
                       .AsNoTracking()
                       .Where(m => m.FromId == UserId || m.ToId == UserId)
                       .Select(m => new { From = m.FromId, To = m.ToId })
                       .Distinct()
                       .ToListAsync(cancellationToken);

            var uniqueUserIds = new HashSet<int>();
            uniqueUsers.ForEach(u =>
            {
                if (u.From != UserId)
                    uniqueUserIds.Add(u.From);
                if (u.To != UserId)
                    uniqueUserIds.Add(u.To);
            });
            if (uniqueUserIds.Count > 0)
            {
                var allowedUserIds = await GetAllowedChatPartnerIdsAsync(UserId, role, cancellationToken);
                uniqueUserIds.IntersectWith(allowedUserIds);

                chatUsers = await _dataContext.Users
                                    .AsNoTracking()
                                    .Where(u => uniqueUserIds.Contains(u.Id) && u.IsActive && u.IsApproved)
                                    .Select(u => new UserDto(u.Id, u.Name, false, u.Role))
                                    .ToListAsync(cancellationToken);
            }
            return chatUsers;
        }

        private async Task<HashSet<int>> GetAllowedChatPartnerIdsAsync(int userId, string userRole, CancellationToken cancellationToken)
        {
            var role = userRole ?? string.Empty;

            if (string.Equals(role, "Child", StringComparison.OrdinalIgnoreCase))
            {
                var classIds = await _dataContext.ClassRoomStudents
                    .AsNoTracking()
                    .Where(cs => cs.StudentId == userId)
                    .Select(cs => cs.ClassRoomId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (classIds.Count == 0)
                {
                    return new HashSet<int>();
                }

                var classmateIds = await _dataContext.ClassRoomStudents
                    .AsNoTracking()
                    .Where(cs => classIds.Contains(cs.ClassRoomId) && cs.StudentId != userId)
                    .Select(cs => cs.StudentId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var teacherIds = await _dataContext.ClassRooms
                    .AsNoTracking()
                    .Where(c => classIds.Contains(c.Id) && c.TeacherId.HasValue)
                    .Select(c => c.TeacherId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                return classmateIds.Concat(teacherIds).ToHashSet();
            }

            if (string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                var classIds = await _dataContext.ClassRooms
                    .AsNoTracking()
                    .Where(c => c.TeacherId == userId)
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);

                if (classIds.Count == 0)
                {
                    return new HashSet<int>();
                }

                var studentIds = await _dataContext.ClassRoomStudents
                    .AsNoTracking()
                    .Where(cs => classIds.Contains(cs.ClassRoomId))
                    .Select(cs => cs.StudentId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                return studentIds.ToHashSet();
            }

            return await _dataContext.Users
                .AsNoTracking()
                .Where(u => u.Id != userId && u.IsActive && u.IsApproved)
                .Select(u => u.Id)
                .ToHashSetAsync(cancellationToken);
        }

    }
}
