using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
public sealed class AdminController : ControllerBase
{
    private readonly DataContext _dataContext;

    public AdminController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _dataContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.AddedOn)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Name,
                u.Username,
                u.Role,
                u.IsApproved,
                u.IsActive,
                u.AddedOn
            ))
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] AdminCreateUserDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Name, Username and Password are required.");
        }

        var role = (dto.Role ?? string.Empty).Trim();
        var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Child", "Parent", "Teacher", "Admin" };
        if (!allowedRoles.Contains(role))
        {
            return BadRequest("Invalid role. Allowed: Child, Parent, Teacher, Admin.");
        }

        var usernameExists = await _dataContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == dto.Username, cancellationToken);
        if (usernameExists)
        {
            return BadRequest("Username already exists.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Username = dto.Username.Trim(),
            Password = dto.Password,
            Role = role,
            IsApproved = dto.IsApproved,
            IsActive = dto.IsActive,
            AddedOn = DateTime.UtcNow
        };

        await _dataContext.Users.AddAsync(user, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);

        return Ok(new AdminUserDto(
            user.Id,
            user.Name,
            user.Username,
            user.Role,
            user.IsApproved,
            user.IsActive,
            user.AddedOn
        ));
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var role = (dto.Role ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("Role is required.");
        }

        var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Child", "Parent", "Teacher", "Admin" };
        if (!allowedRoles.Contains(role))
        {
            return BadRequest("Invalid role. Allowed: Child, Parent, Teacher, Admin.");
        }

        user.Role = role;
        user.IsApproved = dto.IsApproved;
        user.IsActive = dto.IsActive;

        await _dataContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var otherActiveAdmins = await _dataContext.Users
                .AsNoTracking()
                .CountAsync(u => u.Id != id && u.IsActive && u.Role == "Admin", cancellationToken);

            if (otherActiveAdmins == 0)
            {
                return BadRequest("Cannot delete the last active admin.");
            }
        }

        user.IsActive = false;
        user.IsApproved = false;
        await _dataContext.SaveChangesAsync(cancellationToken);

        return NoContent();
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

        var adminUser = new KidSafeApp.Backend.Data.Entities.User
        {
            Name = "School Admin",
            Username = "admin",
            Password = "admin",
            AddedOn = DateTime.Now,
            Role = "Admin",
            IsApproved = true,
            IsActive = true
        };

        await _dataContext.Users.AddAsync(adminUser, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);

        return Ok(new { adminUser.Username, adminUser.Password });
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

    [HttpPost("classrooms/{classRoomId:int}/students")]
    public async Task<IActionResult> AssignStudentToClassRoom(int classRoomId, [FromBody] AdminAssignStudentDto dto, CancellationToken cancellationToken)
    {
        var room = await _dataContext.ClassRooms
            .FirstOrDefaultAsync(c => c.Id == classRoomId, cancellationToken);
        if (room is null)
        {
            return NotFound("Classroom not found.");
        }

        var student = await _dataContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == dto.StudentId && u.Role == "Child", cancellationToken);
        if (student is null)
        {
            return BadRequest("Student does not exist or is not a child account.");
        }

        var exists = await _dataContext.ClassRoomStudents
            .AnyAsync(x => x.ClassRoomId == classRoomId && x.StudentId == dto.StudentId, cancellationToken);
        if (exists)
        {
            return Ok();
        }

        await _dataContext.ClassRoomStudents.AddAsync(new ClassRoomStudent
        {
            ClassRoomId = classRoomId,
            StudentId = dto.StudentId,
            AssignedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dataContext.SaveChangesAsync(cancellationToken);
        return Ok();
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
        return NoContent();
    }

    [HttpPut("classrooms/{classRoomId:int}/teacher")]
    public async Task<IActionResult> AssignTeacherToClassRoom(int classRoomId, [FromBody] AdminAssignTeacherDto dto, CancellationToken cancellationToken)
    {
        var classRoom = await _dataContext.ClassRooms
            .FirstOrDefaultAsync(c => c.Id == classRoomId, cancellationToken);
        if (classRoom is null)
        {
            return NotFound("Classroom not found.");
        }

        var teacherExists = await _dataContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == dto.TeacherId && u.Role == "Teacher" && u.IsActive, cancellationToken);
        if (!teacherExists)
        {
            return BadRequest("Teacher does not exist.");
        }

        classRoom.TeacherId = dto.TeacherId;
        await _dataContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("classrooms/{classRoomId:int}/courses")]
    public async Task<IActionResult> AssignCourseToClassRoom(int classRoomId, [FromBody] AdminAssignCourseToClassDto dto, CancellationToken cancellationToken)
    {
        var classRoom = await _dataContext.ClassRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classRoomId, cancellationToken);
        if (classRoom is null)
        {
            return NotFound("Classroom not found.");
        }

        var course = await _dataContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.CourseId, cancellationToken);
        if (course is null)
        {
            return BadRequest("Course does not exist.");
        }

        if (!course.IsPublished)
        {
            return BadRequest("Only published courses can be assigned to class.");
        }

        var existing = await _dataContext.ClassRoomCourseAssignments
            .FirstOrDefaultAsync(a => a.ClassRoomId == classRoomId && a.CourseId == dto.CourseId, cancellationToken);

        if (existing is null)
        {
            await _dataContext.ClassRoomCourseAssignments.AddAsync(new ClassRoomCourseAssignment
            {
                ClassRoomId = classRoomId,
                CourseId = dto.CourseId,
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
                .FirstOrDefaultAsync(a => a.CourseId == dto.CourseId && a.ChildId == studentId, cancellationToken);

            if (studentCourse is null)
            {
                await _dataContext.CourseAssignments.AddAsync(new CourseAssignment
                {
                    CourseId = dto.CourseId,
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
        return Ok();
    }
}
