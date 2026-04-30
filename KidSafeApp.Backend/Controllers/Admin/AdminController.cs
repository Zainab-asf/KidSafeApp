using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Backend.Domain.Auth;
using KidSafeApp.Backend.Hubs;
using KidSafeApp.Backend.Services;
using KidSafeApp.Backend.Services.Users;
using KidSafeApp.Shared.Chat;
using KidSafeApp.Shared.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace KidSafeApp.Backend.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminController : ControllerBase
{
    private readonly DataContext _dataContext;
    private readonly IUserService _userService;
    private readonly IHubContext<ChatHub, IChatHubClient> _chatHubContext;

    public AdminController(DataContext dataContext, IUserService userService, IHubContext<ChatHub, IChatHubClient> chatHubContext)
    {
        _dataContext = dataContext;
        _userService = userService;
        _chatHubContext = chatHubContext;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var query = new AdminUsersQueryDto { PageNumber = 1, PageSize = 500 };
        var users = await _userService.GetUsersAsync(query, cancellationToken);
        return Ok(users.Items.ToList());
    }

    [HttpGet("users/paged")]
    public async Task<ActionResult<PagedResultDto<AdminUserDto>>> GetUsersPaged([FromQuery] AdminUsersQueryDto query, CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(query, cancellationToken);
        return Ok(users);
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] AdminCreateUserDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(dto, cancellationToken);

            // Keep child/class workflow transactional from admin perspective:
            // create account + map to class + apply class course mappings.
            if (string.Equals(dto.Role, Roles.Child, StringComparison.OrdinalIgnoreCase) && dto.ClassRoomId.HasValue && dto.ClassRoomId.Value > 0)
            {
                var didChangeRoster = await AssignStudentToClassRoomCoreAsync(dto.ClassRoomId.Value, createdUser.Id, cancellationToken);
                await BackfillStudentCoursesFromClassRoomAsync(dto.ClassRoomId.Value, createdUser.Id, cancellationToken);

                if (dto.CourseId.HasValue && dto.CourseId.Value > 0)
                {
                    await AssignCourseToClassRoomCoreAsync(dto.ClassRoomId.Value, dto.CourseId.Value, cancellationToken);
                }

                if (didChangeRoster)
                {
                    await NotifyRosterUpdatedAsync(dto.ClassRoomId.Value);
                }
            }
            else if (string.Equals(dto.Role, Roles.Teacher, StringComparison.OrdinalIgnoreCase) && dto.ClassRoomId.HasValue && dto.ClassRoomId.Value > 0)
            {
                var didChangeRoster = await AssignTeacherToClassRoomCoreAsync(dto.ClassRoomId.Value, createdUser.Id, cancellationToken);
                if (didChangeRoster)
                {
                    await NotifyRosterUpdatedAsync(dto.ClassRoomId.Value);
                }
            }

            return Ok(createdUser);
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.UpdateUserAsync(id, dto, cancellationToken);
            return NoContent();
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteUserAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPost("bootstrap")]
    [AllowAnonymous]
    public async Task<IActionResult> BootstrapAdmin([FromQuery] string key, CancellationToken cancellationToken)
    {
        // One-time helper endpoint for school/dev to create the first admin.
        // Configure the key via appsettings: Admin:BootstrapKey
        var expected = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Admin:BootstrapKey"];
        if (string.IsNullOrWhiteSpace(expected) || !string.Equals(expected, key, StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var anyAdmin = await _dataContext.Users.AsNoTracking().AnyAsync(u => u.Role == "Admin", cancellationToken);
        if (anyAdmin)
        {
            return BadRequest("Admin already exists.");
        }

        var created = await _userService.CreateUserAsync(new AdminCreateUserDto
        {
            Name = "School Admin",
            Username = "admin",
            Password = "admin",
            Role = Roles.Admin,
            IsApproved = true,
            IsActive = true
        }, cancellationToken);

        return Ok(new { created.Username });
    }

    [HttpGet("classrooms")]
    public async Task<ActionResult<List<AdminClassRoomDto>>> GetClassRooms(CancellationToken cancellationToken)
    {
        var rooms = await _dataContext.ClassRooms
            .AsNoTracking()
            .Include(c => c.Students)
            .OrderBy(c => c.Name)
            .Select(c => new AdminClassRoomDto
            {
                Id = c.Id,
                Name = c.Name,
                Grade = c.Grade,
                Section = c.Section,
                StudentCount = c.Students.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(rooms);
    }

    [HttpGet("classrooms/details")]
    public async Task<ActionResult<List<AdminClassRoomDetailDto>>> GetClassRoomDetails(CancellationToken cancellationToken)
    {
        var rooms = await _dataContext.ClassRooms
            .AsNoTracking()
            .Include(c => c.Teacher)
            .Include(c => c.Students)
                .ThenInclude(cs => cs.Student)
            .Include(c => c.CourseAssignments)
                .ThenInclude(a => a.Course)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var response = rooms.Select(c => new AdminClassRoomDetailDto
        {
            Id = c.Id,
            Name = c.Name,
            Grade = c.Grade,
            Section = c.Section,
            TeacherId = c.TeacherId,
            TeacherName = c.Teacher?.Name,
            Students = c.Students
                .OrderBy(s => s.Student.Name)
                .Select(s => new AdminClassRoomStudentDto
                {
                    Id = s.StudentId,
                    Name = s.Student.Name,
                    Username = s.Student.Username
                }).ToList(),
            Courses = c.CourseAssignments
                .Where(a => a.IsActive)
                .OrderBy(a => a.Course.Title)
                .Select(a => new AdminCourseLiteDto
                {
                    Id = a.CourseId,
                    Title = a.Course.Title,
                    IsPublished = a.Course.IsPublished
                }).ToList()
        }).ToList();

        return Ok(response);
    }

    [HttpGet("teachers")]
    public async Task<ActionResult<List<AdminUserDto>>> GetTeachers(CancellationToken cancellationToken)
    {
        var teachers = await _dataContext.Users
            .AsNoTracking()
            .Where(u => u.Role == "Teacher" && u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => new AdminUserDto(u.Id, u.Name, u.Username, u.Role, u.IsApproved, u.IsActive, u.AddedOn))
            .ToListAsync(cancellationToken);

        return Ok(teachers);
    }

    [HttpGet("students")]
    public async Task<ActionResult<List<AdminStudentOverviewDto>>> GetStudents([FromQuery] int? classRoomId = null, CancellationToken cancellationToken = default)
    {
        var students = await _dataContext.Users
            .AsNoTracking()
            .Where(u => u.Role == "Child" && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);

        var studentIds = students.Select(s => s.Id).ToList();

        var mappings = await _dataContext.ClassRoomStudents
            .AsNoTracking()
            .Where(cs => studentIds.Contains(cs.StudentId))
            .Include(cs => cs.ClassRoom)
            .ToListAsync(cancellationToken);

        if (classRoomId.HasValue)
        {
            var filteredStudentIds = mappings
                .Where(m => m.ClassRoomId == classRoomId.Value)
                .Select(m => m.StudentId)
                .Distinct()
                .ToHashSet();

            students = students.Where(s => filteredStudentIds.Contains(s.Id)).ToList();
        }

        var result = students.Select(s => new AdminStudentOverviewDto
        {
            Id = s.Id,
            Name = s.Name,
            Username = s.Username,
            ClassRooms = mappings
                .Where(m => m.StudentId == s.Id)
                .Select(m => string.IsNullOrWhiteSpace(m.ClassRoom.Grade)
                    ? m.ClassRoom.Name
                    : $"{m.ClassRoom.Name} ({m.ClassRoom.Grade}-{m.ClassRoom.Section})")
                .Distinct()
                .OrderBy(x => x)
                .ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("students/{studentId:int}")]
    public async Task<ActionResult<AdminStudentProfileDto>> GetStudentProfile(int studentId, CancellationToken cancellationToken)
    {
        var student = await _dataContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == "Child", cancellationToken);

        if (student is null)
        {
            return NotFound("Student not found.");
        }

        var classMappings = await _dataContext.ClassRoomStudents
            .AsNoTracking()
            .Where(cs => cs.StudentId == studentId)
            .Include(cs => cs.ClassRoom)
            .ToListAsync(cancellationToken);

        var assignedCourseIds = await _dataContext.CourseAssignments
            .AsNoTracking()
            .Where(a => a.ChildId == studentId && a.IsActive)
            .Select(a => a.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var courses = await _dataContext.Courses
            .AsNoTracking()
            .Where(c => assignedCourseIds.Contains(c.Id))
            .OrderBy(c => c.Title)
            .Select(c => new AdminCourseLiteDto
            {
                Id = c.Id,
                Title = c.Title,
                IsPublished = c.IsPublished
            })
            .ToListAsync(cancellationToken);

        var profile = new AdminStudentProfileDto
        {
            Id = student.Id,
            Name = student.Name,
            Username = student.Username,
            IsApproved = student.IsApproved,
            IsActive = student.IsActive,
            AddedOn = student.AddedOn,
            ClassRooms = classMappings
                .Select(m => string.IsNullOrWhiteSpace(m.ClassRoom.Grade)
                    ? m.ClassRoom.Name
                    : $"{m.ClassRoom.Name} ({m.ClassRoom.Grade}-{m.ClassRoom.Section})")
                .Distinct()
                .OrderBy(x => x)
                .ToList(),
            Courses = courses
        };

        return Ok(profile);
    }

    [HttpGet("courses")]
    public async Task<ActionResult<List<AdminCourseLiteDto>>> GetCourses(CancellationToken cancellationToken)
    {
        var courses = await _dataContext.Courses
            .AsNoTracking()
            .OrderBy(c => c.Title)
            .Select(c => new AdminCourseLiteDto
            {
                Id = c.Id,
                Title = c.Title,
                IsPublished = c.IsPublished
            })
            .ToListAsync(cancellationToken);

        return Ok(courses);
    }

    [HttpPost("classrooms")]
    public async Task<ActionResult<AdminClassRoomDto>> CreateClassRoom([FromBody] AdminCreateClassRoomDto dto, CancellationToken cancellationToken)
    {
        var room = new ClassRoom
        {
            Name = dto.Name.Trim(),
            Grade = dto.Grade?.Trim() ?? string.Empty,
            Section = dto.Section?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _dataContext.ClassRooms.AddAsync(room, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);

        return Ok(new AdminClassRoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Grade = room.Grade,
            Section = room.Section,
            StudentCount = 0
        });
    }

    [HttpDelete("classrooms/{classRoomId:int}")]
    public async Task<IActionResult> DeleteClassRoom(int classRoomId, CancellationToken cancellationToken)
    {
        var room = await _dataContext.ClassRooms
            .FirstOrDefaultAsync(c => c.Id == classRoomId, cancellationToken);

        if (room is null)
        {
            return NotFound("Classroom not found.");
        }

        _dataContext.ClassRooms.Remove(room);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("classrooms/{classRoomId:int}/students")]
    public async Task<IActionResult> AssignStudentToClassRoom(int classRoomId, [FromBody] AdminAssignStudentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var didChangeRoster = await AssignStudentToClassRoomCoreAsync(classRoomId, dto.StudentId, cancellationToken);
            await BackfillStudentCoursesFromClassRoomAsync(classRoomId, dto.StudentId, cancellationToken);
            if (didChangeRoster)
            {
                await NotifyRosterUpdatedAsync(classRoomId);
            }

            return Ok();
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpDelete("classrooms/{classRoomId:int}/students/{studentId:int}")]
    public async Task<IActionResult> RemoveStudentFromClassRoom(int classRoomId, int studentId, CancellationToken cancellationToken)
    {
        var mapping = await _dataContext.ClassRoomStudents
            .FirstOrDefaultAsync(x => x.ClassRoomId == classRoomId && x.StudentId == studentId, cancellationToken);

        if (mapping is null)
        {
            return NotFound();
        }

        _dataContext.ClassRoomStudents.Remove(mapping);
        await _dataContext.SaveChangesAsync(cancellationToken);
        await NotifyRosterUpdatedAsync(classRoomId);
        return NoContent();
    }

    [HttpPut("classrooms/{classRoomId:int}/teacher")]
    public async Task<IActionResult> AssignTeacherToClassRoom(int classRoomId, [FromBody] AdminAssignTeacherDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var didChangeRoster = await AssignTeacherToClassRoomCoreAsync(classRoomId, dto.TeacherId, cancellationToken);
            if (didChangeRoster)
            {
                await NotifyRosterUpdatedAsync(classRoomId);
            }

            return NoContent();
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPost("classrooms/{classRoomId:int}/courses")]
    public async Task<IActionResult> AssignCourseToClassRoom(int classRoomId, [FromBody] AdminAssignCourseToClassDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await AssignCourseToClassRoomCoreAsync(classRoomId, dto.CourseId, cancellationToken);
            await NotifyRosterUpdatedAsync(classRoomId);
            return Ok();
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    private async Task<bool> AssignStudentToClassRoomCoreAsync(int classRoomId, int studentId, CancellationToken cancellationToken)
    {
        var roomExists = await _dataContext.ClassRooms
            .AsNoTracking()
            .AnyAsync(c => c.Id == classRoomId, cancellationToken);
        if (!roomExists)
        {
            throw new ServiceException("Classroom not found.", StatusCodes.Status404NotFound);
        }

        var studentExists = await _dataContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == studentId && u.Role == Roles.Child && u.IsActive, cancellationToken);
        if (!studentExists)
        {
            throw new ServiceException("Student does not exist or is not an active child account.", StatusCodes.Status400BadRequest);
        }

        var exists = await _dataContext.ClassRoomStudents
            .AnyAsync(x => x.ClassRoomId == classRoomId && x.StudentId == studentId, cancellationToken);
        if (exists)
        {
            return false;
        }

        await _dataContext.ClassRoomStudents.AddAsync(new ClassRoomStudent
        {
            ClassRoomId = classRoomId,
            StudentId = studentId,
            AssignedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dataContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> AssignTeacherToClassRoomCoreAsync(int classRoomId, int teacherId, CancellationToken cancellationToken)
    {
        var classRoom = await _dataContext.ClassRooms
            .FirstOrDefaultAsync(c => c.Id == classRoomId, cancellationToken);
        if (classRoom is null)
        {
            throw new ServiceException("Classroom not found.", StatusCodes.Status404NotFound);
        }

        var teacherExists = await _dataContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == teacherId && u.Role == Roles.Teacher && u.IsActive, cancellationToken);
        if (!teacherExists)
        {
            throw new ServiceException("Teacher does not exist.", StatusCodes.Status400BadRequest);
        }

        if (classRoom.TeacherId == teacherId)
        {
            return false;
        }

        classRoom.TeacherId = teacherId;
        await _dataContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task AssignCourseToClassRoomCoreAsync(int classRoomId, int courseId, CancellationToken cancellationToken)
    {
        var classRoom = await _dataContext.ClassRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classRoomId, cancellationToken);
        if (classRoom is null)
        {
            throw new ServiceException("Classroom not found.", StatusCodes.Status404NotFound);
        }

        var course = await _dataContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
        if (course is null)
        {
            throw new ServiceException("Course does not exist.", StatusCodes.Status400BadRequest);
        }

        if (!course.IsPublished)
        {
            throw new ServiceException("Only published courses can be assigned to class.", StatusCodes.Status400BadRequest);
        }

        var existing = await _dataContext.ClassRoomCourseAssignments
            .FirstOrDefaultAsync(a => a.ClassRoomId == classRoomId && a.CourseId == courseId, cancellationToken);

        if (existing is null)
        {
            await _dataContext.ClassRoomCourseAssignments.AddAsync(new ClassRoomCourseAssignment
            {
                ClassRoomId = classRoomId,
                CourseId = courseId,
                IsActive = true,
                AssignedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            existing.IsActive = true;
        }

        var studentIds = await _dataContext.ClassRoomStudents
            .AsNoTracking()
            .Where(x => x.ClassRoomId == classRoomId)
            .Select(x => x.StudentId)
            .ToListAsync(cancellationToken);

        foreach (var studentId in studentIds)
        {
            var studentCourse = await _dataContext.CourseAssignments
                .FirstOrDefaultAsync(a => a.CourseId == courseId && a.ChildId == studentId, cancellationToken);

            if (studentCourse is null)
            {
                await _dataContext.CourseAssignments.AddAsync(new CourseAssignment
                {
                    CourseId = courseId,
                    ChildId = studentId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow
                }, cancellationToken);
            }
            else
            {
                studentCourse.IsActive = true;
            }
        }

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    private async Task BackfillStudentCoursesFromClassRoomAsync(int classRoomId, int studentId, CancellationToken cancellationToken)
    {
        var activeClassCourseIds = await _dataContext.ClassRoomCourseAssignments
            .AsNoTracking()
            .Where(x => x.ClassRoomId == classRoomId && x.IsActive)
            .Select(x => x.CourseId)
            .ToListAsync(cancellationToken);

        foreach (var courseId in activeClassCourseIds)
        {
            var existing = await _dataContext.CourseAssignments
                .FirstOrDefaultAsync(x => x.ChildId == studentId && x.CourseId == courseId, cancellationToken);
            if (existing is null)
            {
                await _dataContext.CourseAssignments.AddAsync(new CourseAssignment
                {
                    ChildId = studentId,
                    CourseId = courseId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow
                }, cancellationToken);
            }
            else
            {
                existing.IsActive = true;
            }
        }

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    private Task NotifyRosterUpdatedAsync(int classRoomId)
    {
        return _chatHubContext.Clients.All.RosterUpdated(classRoomId);
    }
}
