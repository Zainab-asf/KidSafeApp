using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers;

[ApiController]
[Route("api/child/learning")]
public sealed class ChildLearningController : ControllerBase
{
    private readonly DataContext _context;

    public ChildLearningController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("courses")]
    public async Task<ActionResult<List<ChildCourseDto>>> GetPublishedCourses(CancellationToken cancellationToken)
    {
        var childId = await GetActingChildIdAsync(cancellationToken);

        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.IsPublished)
            .Include(c => c.CourseLessons.Where(cl => cl.IsPublished))
            .OrderBy(c => c.Title)
            .ToListAsync(cancellationToken);

        var courseIds = courses.Select(c => c.Id).ToList();

        var progressByCourse = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.ChildId == childId && courseIds.Contains(lp.CourseLesson.CourseId))
            .Select(lp => new { lp.CourseLesson.CourseId, lp.IsCompleted })
            .ToListAsync(cancellationToken);

        var dtos = courses.Select(c =>
        {
            var total = c.CourseLessons.Count;
            var completed = progressByCourse.Count(p => p.CourseId == c.Id && p.IsCompleted);
            var percent = total == 0 ? 0 : (int)Math.Round((completed * 100d) / total);

            return new ChildCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Subject = c.Subject,
                DifficultyLevel = c.DifficultyLevel,
                LessonCount = total,
                CompletedLessons = completed,
                PercentageComplete = percent
            };
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("courses/{courseId:int}/lessons")]
    public async Task<ActionResult<List<CourseLessonDto>>> GetCourseLessons(int courseId, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId && c.IsPublished)
            .Include(c => c.CourseLessons.Where(cl => cl.IsPublished))
                .ThenInclude(cl => cl.Lesson)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        var lessonDtos = course.CourseLessons
            .OrderBy(cl => cl.SortOrder)
            .Select(cl => new CourseLessonDto
            {
                CourseLessonId = cl.Id,
                LessonId = cl.LessonId,
                LessonTitle = cl.Lesson?.Title ?? string.Empty,
                Subject = cl.Lesson?.Subject,
                DifficultyLevel = cl.Lesson?.DifficultyLevel ?? 0,
                ThumbnailUrl = cl.Lesson?.ThumbnailUrl,
                PdfUrl = cl.PdfUrl,
                SortOrder = cl.SortOrder,
                IsPublished = cl.IsPublished
            })
            .ToList();

        return Ok(lessonDtos);
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
