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
        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            if (User.IsInRole("Child"))
            {
                var classIds = await _dataContext.ClassRoomStudents
                    .AsNoTracking()
                    .Where(cs => cs.StudentId == UserId)
                    .Select(cs => cs.ClassRoomId)
                    .Distinct()
                    .ToListAsync();

                if (classIds.Count == 0)
                {
                    return Enumerable.Empty<UserDto>();
                }

                var classmateIds = await _dataContext.ClassRoomStudents
                    .AsNoTracking()
                    .Where(cs => classIds.Contains(cs.ClassRoomId) && cs.StudentId != UserId)
                    .Select(cs => cs.StudentId)
                    .Distinct()
                    .ToListAsync();

                if (classmateIds.Count == 0)
                {
                    return Enumerable.Empty<UserDto>();
                }

                return await _dataContext.Users
                    .AsNoTracking()
                    .Where(u => classmateIds.Contains(u.Id) && u.IsActive && u.IsApproved)
                    .Select(u => new UserDto(u.Id, u.Name, false, u.Role))
                    .ToListAsync();
            }

            return await _dataContext.Users
                .AsNoTracking()
                .Where(u => u.Id != UserId && u.IsActive && u.IsApproved)
                .Select(u => new UserDto(u.Id, u.Name, false, u.Role))
                .ToListAsync();
        }

        [HttpGet("chats")]
        public async Task<IEnumerable<UserDto>> GetUserChats(CancellationToken cancellationToken)
        {
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
                if (User.IsInRole("Child"))
                {
                    var classIds = await _dataContext.ClassRoomStudents
                        .AsNoTracking()
                        .Where(cs => cs.StudentId == UserId)
                        .Select(cs => cs.ClassRoomId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    var classmateIds = await _dataContext.ClassRoomStudents
                        .AsNoTracking()
                        .Where(cs => classIds.Contains(cs.ClassRoomId) && cs.StudentId != UserId)
                        .Select(cs => cs.StudentId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    uniqueUserIds.IntersectWith(classmateIds);
                }

                chatUsers = await _dataContext.Users
                                    .AsNoTracking()
                                    .Where(u => uniqueUserIds.Contains(u.Id) && u.IsActive && u.IsApproved)
                                    .Select(u => new UserDto(u.Id, u.Name, false, u.Role))
                                    .ToListAsync(cancellationToken);
            }
            return chatUsers;
        }

    }
}
