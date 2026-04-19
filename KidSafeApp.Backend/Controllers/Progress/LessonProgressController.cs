using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers.Progress;

[ApiController]
[Route("api/learning/progress")]
public sealed class LessonProgressController : ControllerBase
{
    private readonly DataContext _context;

    public LessonProgressController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<ActionResult<List<LessonProgressDto>>> GetCourseProgress(int courseId, CancellationToken cancellationToken)
    {
        var childId = await GetActingChildIdAsync(cancellationToken);

        var rows = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.ChildId == childId && lp.CourseLesson.CourseId == courseId)
            .Select(lp => new LessonProgressDto
            {
                CourseLessonId = lp.CourseLessonId,
                IsStarted = lp.IsStarted,
                IsCompleted = lp.IsCompleted,
                PercentageComplete = lp.PercentageComplete,
                TimeSpentSeconds = lp.TimeSpentSeconds,
                StartedAt = lp.StartedAt,
                CompletedAt = lp.CompletedAt,
                LastAccessedAt = lp.LastAccessedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpPut]
    public async Task<ActionResult<LessonProgressDto>> Upsert([FromBody] UpdateLessonProgressDto dto, CancellationToken cancellationToken)
    {
        var childId = await GetActingChildIdAsync(cancellationToken);

        var courseLesson = await _context.CourseLessons
            .AsNoTracking()
            .Include(cl => cl.Course)
            .FirstOrDefaultAsync(cl => cl.Id == dto.CourseLessonId, cancellationToken);

        if (courseLesson is null || !courseLesson.IsPublished || !courseLesson.Course.IsPublished)
        {
            return BadRequest("Lesson is not available.");
        }

        var entity = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.CourseLessonId == dto.CourseLessonId && lp.ChildId == childId, cancellationToken);

        var now = DateTime.UtcNow;

        if (entity is null)
        {
            entity = new LessonProgress
            {
                CourseLessonId = dto.CourseLessonId,
                ChildId = childId,
                IsStarted = true,
                StartedAt = now,
                IsCompleted = dto.IsCompleted,
                CompletedAt = dto.IsCompleted ? now : null,
                PercentageComplete = Math.Clamp(dto.PercentageComplete, 0, 100),
                TimeSpentSeconds = Math.Max(0, dto.TimeSpentSeconds),
                LastAccessedAt = now,
                UpdatedAt = now
            };

            _context.LessonProgresses.Add(entity);
        }
        else
        {
            entity.IsStarted = true;
            entity.StartedAt ??= now;
            entity.PercentageComplete = Math.Clamp(dto.PercentageComplete, 0, 100);
            entity.TimeSpentSeconds = Math.Max(0, dto.TimeSpentSeconds);
            entity.IsCompleted = dto.IsCompleted || entity.PercentageComplete >= 100;
            entity.CompletedAt = entity.IsCompleted ? (entity.CompletedAt ?? now) : null;
            entity.LastAccessedAt = now;
            entity.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new LessonProgressDto
        {
            CourseLessonId = entity.CourseLessonId,
            IsStarted = entity.IsStarted,
            IsCompleted = entity.IsCompleted,
            PercentageComplete = entity.PercentageComplete,
            TimeSpentSeconds = entity.TimeSpentSeconds,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            LastAccessedAt = entity.LastAccessedAt
        });
    }

    private async Task<int> GetActingChildIdAsync(CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out var id) && id > 0)
        {
            return id;
        }

        var existingChildId = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == "Child" && u.IsActive)
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingChildId > 0)
        {
            return existingChildId;
        }

        var demoChild = new User
        {
            Name = "Child Demo",
            Username = "child_demo",
            Password = "child_demo",
            Role = "Child",
            IsApproved = true,
            IsActive = true,
            AddedOn = DateTime.UtcNow
        };

        _context.Users.Add(demoChild);
        await _context.SaveChangesAsync(cancellationToken);
        return demoChild.Id;
    }
}
