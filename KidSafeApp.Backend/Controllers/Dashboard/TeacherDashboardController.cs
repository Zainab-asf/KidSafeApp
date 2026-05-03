using KidSafeApp.Backend.Controllers.Common;
using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Domain.Auth;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Shared.DTOs.Courses;
using KidSafeApp.Shared.DTOs.Dashboard;
using KidSafeApp.Shared.DTOs.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Controllers.Dashboard;

[ApiController]
[Route("api/dashboard/teacher")]
[Authorize(Roles = Roles.Teacher)]
public sealed class TeacherDashboardController : BaseController
{
    private readonly DataContext _context;

    public TeacherDashboardController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<TeacherDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var teacher = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == UserId && u.Role == Roles.Teacher, cancellationToken);

        if (teacher is null)
        {
            return NotFound("Teacher user not found.");
        }

        var courseIds = await _context.Courses
            .AsNoTracking()
            .Where(c => c.TeacherId == UserId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.TeacherId == UserId)
            .Include(c => c.CourseLessons)
                .ThenInclude(cl => cl.Lesson)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var lessonCount = await _context.CourseLessons
            .AsNoTracking()
            .Where(cl => courseIds.Contains(cl.CourseId))
            .CountAsync(cancellationToken);

        var progressRows = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => courseIds.Contains(lp.CourseLesson.CourseId))
            .Select(lp => new { lp.IsCompleted, lp.PercentageComplete })
            .ToListAsync(cancellationToken);

        var summary = new TeacherSummaryDto
        {
            TotalCourses = courseIds.Count,
            PublishedCourses = courses.Count(c => c.IsPublished),
            TotalLessons = lessonCount,
            PublishedLessons = courses.Sum(c => c.CourseLessons.Count(cl => cl.IsPublished)),
            CompletedLessonSubmissions = progressRows.Count(r => r.IsCompleted),
            PendingLessonSubmissions = progressRows.Count(r => !r.IsCompleted && r.PercentageComplete > 0),
            AverageProgressPercent = progressRows.Count == 0
                ? 0
                : Math.Round(progressRows.Average(r => r.PercentageComplete), 1)
        };

        var weekly = await BuildWeeklyAsync(courseIds, cancellationToken);

        var dto = new TeacherDashboardDto
        {
            Summary = summary,
            Courses = courses.Select(MapCourse).ToList(),
            Weekly = weekly
        };

        return Ok(dto);
    }

    private async Task<List<WeeklyPointDto>> BuildWeeklyAsync(List<int> courseIds, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var lastWeekStart = today.AddDays(-6);

        var progressRows = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => courseIds.Contains(lp.CourseLesson.CourseId))
            .Where(lp => lp.LastAccessedAt >= lastWeekStart)
            .Select(lp => new { lp.LastAccessedAt, lp.IsCompleted })
            .ToListAsync(cancellationToken);

        var points = new List<WeeklyPointDto>();

        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var dayRows = progressRows.Where(r => r.LastAccessedAt.Date == day).ToList();
            var total = dayRows.Count;
            var completed = dayRows.Count(r => r.IsCompleted);
            var pending = total - completed;

            points.Add(new WeeklyPointDto
            {
                Day = day.ToString("ddd"),
                Safe = total == 0 ? 0 : (int)Math.Round((completed * 100d) / total),
                Flagged = total == 0 ? 0 : (int)Math.Round((pending * 100d) / total),
                Blocked = 0
            });
        }

        return points;
    }

    private static CourseDto MapCourse(Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Subject = course.Subject,
            DifficultyLevel = course.DifficultyLevel,
            IsPublished = course.IsPublished,
            CreatedAt = course.CreatedAt,
            Lessons = course.CourseLessons
                .OrderBy(x => x.SortOrder)
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
                }).ToList()
        };
    }
}
