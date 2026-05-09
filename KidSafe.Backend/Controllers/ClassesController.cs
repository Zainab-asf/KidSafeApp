using System.Security.Claims;
using KidSafe.Backend.Data;
using KidSafe.Backend.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafe.Backend.Controllers;

[ApiController]
[Route("classes")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClassesController(AppDbContext db) => _db = db;

    // ── GET /classes ─────────────────────────────────────────────
    // Admin → all  |  Teacher → own  |  Student → enrolled

    [HttpGet]
    public async Task<IActionResult> GetClasses()
    {
        var uid  = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(ClaimTypes.Role)!.Value;

        var q = _db.Classes
            .Include(c => c.Teacher)
            .Include(c => c.Students)
            .AsQueryable();

        q = role switch
        {
            "Admin"   => q,
            "Teacher" => q.Where(c => c.TeacherId == uid),
            _         => q.Where(c => c.Students.Any(s => s.StudentId == uid))
        };

        var list = await q
            .Include(c => c.Content)
            .Select(c => new
            {
                c.Id, c.Name, c.Section, c.Subject,
                Teacher      = c.Teacher == null ? null : c.Teacher.DisplayName,
                c.TeacherId,
                StudentCount = c.Students.Count,
                ContentCount = c.Content.Count,
                c.CreatedAt
            }).ToListAsync();

        return Ok(list);
    }

    // ── GET /classes/{id} ────────────────────────────────────────

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetClass(int id)
    {
        var cls = await _db.Classes
            .Include(c => c.Teacher)
            .Include(c => c.Students).ThenInclude(cs => cs.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cls == null) return NotFound();

        return Ok(new
        {
            cls.Id, cls.Name, cls.Section, cls.Subject,
            Teacher  = cls.Teacher?.DisplayName,
            cls.TeacherId,
            Students = cls.Students.Select(cs => new
            {
                cs.Student.Id,
                cs.Student.DisplayName,
                cs.Student.Email,
                cs.JoinedAt
            }),
            cls.CreatedAt
        });
    }

    // ── POST /classes  (Admin only) ───────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateClass([FromBody] ClassDto dto)
    {
        var cls = new Class
        {
            Name      = dto.Name,
            Section   = dto.Section,
            Subject   = dto.Subject,
            TeacherId = dto.TeacherId
        };
        _db.Classes.Add(cls);
        await _db.SaveChangesAsync();
        return Ok(new { cls.Id, cls.Name, cls.Section, cls.Subject });
    }

    // ── PUT /classes/{id}  (Admin) ────────────────────────────────

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassDto dto)
    {
        var cls = await _db.Classes.FindAsync(id);
        if (cls == null) return NotFound();

        cls.Name      = dto.Name;
        cls.Section   = dto.Section;
        cls.Subject   = dto.Subject;
        cls.TeacherId = dto.TeacherId;
        await _db.SaveChangesAsync();
        return Ok(new { cls.Id, cls.Name, cls.Section });
    }

    // ── DELETE /classes/{id}  (Admin) ─────────────────────────────

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteClass(int id)
    {
        var cls = await _db.Classes.FindAsync(id);
        if (cls == null) return NotFound();
        _db.Classes.Remove(cls);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /classes/{id}/students  (Admin) ──────────────────────

    [HttpPost("{id:int}/students")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddStudent(int id, [FromBody] StudentActionDto dto)
    {
        if (await _db.ClassStudents.AnyAsync(cs => cs.ClassId == id && cs.StudentId == dto.StudentId))
            return Conflict("Already enrolled.");

        _db.ClassStudents.Add(new ClassStudent { ClassId = id, StudentId = dto.StudentId });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Student added." });
    }

    // ── DELETE /classes/{id}/students/{studentId}  (Admin) ────────

    [HttpDelete("{id:int}/students/{studentId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveStudent(int id, int studentId)
    {
        var link = await _db.ClassStudents
            .FirstOrDefaultAsync(cs => cs.ClassId == id && cs.StudentId == studentId);
        if (link == null) return NotFound();
        _db.ClassStudents.Remove(link);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /classes/classmates  (Child: list classmates in same class) ──
    [HttpGet("classmates")]
    [Authorize(Roles = "Child")]
    public async Task<IActionResult> GetMyClassmates()
    {
        var uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var myClass = await _db.ClassStudents
            .Where(cs => cs.StudentId == uid)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.Students)
                    .ThenInclude(s => s.Student)
            .Select(cs => cs.Class)
            .FirstOrDefaultAsync();

        if (myClass is null)
            return Ok(new { className = (string?)null, section = (string?)null,
                            classmates = Array.Empty<object>() });

        var classmates = myClass.Students
            .Where(s => s.StudentId != uid)
            .Select(s => new
            {
                s.Student.Id,
                s.Student.DisplayName,
                Avatar = s.Student.AvatarEmoji ?? "😊"
            }).ToList();

        return Ok(new { className = myClass.Name, section = myClass.Section, classmates });
    }

    // ── GET /classes/{id}/messages  (class group chat history) ────

    [HttpGet("{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int take = 50)
    {
        var msgs = await _db.ChatMessages
            .Where(m => m.ClassId == id)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.Timestamp)
            .Take(take)
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                m.Id, m.SenderId,
                SenderName = m.Sender.DisplayName,
                SenderEmoji = m.Sender.AvatarEmoji ?? "😊",
                Content    = m.Label == "Watch" ? m.Masked : m.Content,
                m.Label, m.Score, m.Timestamp
            })
            .ToListAsync();

        return Ok(msgs);
    }
}

public record ClassDto(string Name, string Section, string Subject, int? TeacherId);
public record StudentActionDto(int StudentId);
