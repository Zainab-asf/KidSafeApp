using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers;

[ApiController]
[Route("api/teacher/courses")]
public sealed class TeacherCoursesController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _environment;

    public TeacherCoursesController(DataContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseDto>>> GetMyCourses(CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId)
            .Include(c => c.CourseLessons)
                .ThenInclude(cl => cl.Lesson)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(courses.Select(MapCourse).ToList());
    }

    [HttpGet("summary")]
    public async Task<ActionResult<TeacherSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var totalCourses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId)
            .CountAsync(cancellationToken);

        var publishedCourses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId && c.IsPublished)
            .CountAsync(cancellationToken);

        var courseIds = await _context.Courses
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var totalLessons = await _context.CourseLessons
            .AsNoTracking()
            .Where(cl => courseIds.Contains(cl.CourseId))
            .CountAsync(cancellationToken);

        var publishedLessons = await _context.CourseLessons
            .AsNoTracking()
            .Where(cl => courseIds.Contains(cl.CourseId) && cl.IsPublished)
            .CountAsync(cancellationToken);

        var progressRows = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => courseIds.Contains(lp.CourseLesson.CourseId))
            .Select(lp => new { lp.IsCompleted, lp.PercentageComplete })
            .ToListAsync(cancellationToken);

        var summary = new TeacherSummaryDto
        {
            TotalCourses = totalCourses,
            PublishedCourses = publishedCourses,
            TotalLessons = totalLessons,
            PublishedLessons = publishedLessons,
            CompletedLessonSubmissions = progressRows.Count(r => r.IsCompleted),
            PendingLessonSubmissions = progressRows.Count(r => !r.IsCompleted && r.PercentageComplete > 0),
            AverageProgressPercent = progressRows.Count == 0 ? 0 : Math.Round(progressRows.Average(r => r.PercentageComplete), 1)
        };

        return Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CreateCourseDto dto, CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var course = new Course
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Subject = dto.Subject?.Trim(),
            DifficultyLevel = dto.DifficultyLevel,
            TeacherId = teacherId,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetMyCourses), new { id = course.Id }, MapCourse(course));
    }

    [HttpPut("{courseId:int}")]
    public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] UpdateCourseDto dto, CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacherId, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        course.Title = dto.Title.Trim();
        course.Description = dto.Description?.Trim();
        course.Subject = dto.Subject?.Trim();
        course.DifficultyLevel = dto.DifficultyLevel;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("{courseId:int}/publish")]
    public async Task<IActionResult> SetPublishState(int courseId, [FromQuery] bool isPublished, CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var course = await _context.Courses
            .Include(c => c.CourseLessons)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacherId, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        if (isPublished && !course.CourseLessons.Any())
        {
            return BadRequest("Course must contain at least one lesson before publishing.");
        }

        course.IsPublished = isPublished;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{courseId:int}/lessons")]
    public async Task<ActionResult<CourseLessonDto>> AddLessonToCourse(int courseId, [FromBody] AddCourseLessonDto dto, CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacherId, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == dto.LessonId, cancellationToken);

        if (lesson is null)
        {
            return BadRequest("Lesson does not exist.");
        }

        var exists = await _context.CourseLessons
            .AnyAsync(cl => cl.CourseId == courseId && cl.LessonId == dto.LessonId, cancellationToken);

        if (exists)
        {
            return Conflict("Lesson already added to this course.");
        }

        var courseLesson = new CourseLesson
        {
            CourseId = courseId,
            LessonId = dto.LessonId,
            SortOrder = dto.SortOrder,
            IsPublished = false
        };

        _context.CourseLessons.Add(courseLesson);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new CourseLessonDto
        {
            CourseLessonId = courseLesson.Id,
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            Subject = lesson.Subject,
            DifficultyLevel = lesson.DifficultyLevel,
            ThumbnailUrl = lesson.ThumbnailUrl,
            PdfUrl = courseLesson.PdfUrl,
            SortOrder = courseLesson.SortOrder,
            IsPublished = courseLesson.IsPublished
        });
    }

    [HttpPost("{courseId:int}/lessons/{courseLessonId:int}/content")]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> UploadLessonPdf(int courseId, int courseLessonId, IFormFile file, CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var isOwner = await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.TeacherId == teacherId, cancellationToken);

        if (!isOwner)
        {
            return NotFound();
        }

        var courseLesson = await _context.CourseLessons
            .FirstOrDefaultAsync(cl => cl.Id == courseLessonId && cl.CourseId == courseId, cancellationToken);

        if (courseLesson is null)
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF files are allowed.");
        }

        var root = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadFolder = Path.Combine(root, "uploads", "courses", courseId.ToString(), "lessons", courseLessonId.ToString());
        Directory.CreateDirectory(uploadFolder);

        var safeFileName = $"content_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var fullPath = Path.Combine(uploadFolder, safeFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        courseLesson.PdfUrl = $"/uploads/courses/{courseId}/lessons/{courseLessonId}/{safeFileName}";
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { courseLesson.PdfUrl });
    }

    [HttpPut("{courseId:int}/lessons/{courseLessonId:int}/publish")]
    public async Task<IActionResult> SetLessonPublishState(int courseId, int courseLessonId, [FromQuery] bool isPublished, CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var isOwner = await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.TeacherId == teacherId, cancellationToken);

        if (!isOwner)
        {
            return NotFound();
        }

        var courseLesson = await _context.CourseLessons
            .FirstOrDefaultAsync(cl => cl.Id == courseLessonId && cl.CourseId == courseId, cancellationToken);

        if (courseLesson is null)
        {
            return NotFound();
        }

        courseLesson.IsPublished = isPublished;
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{courseId:int}/lessons/{courseLessonId:int}")]
    public async Task<IActionResult> RemoveLessonFromCourse(int courseId, int courseLessonId, CancellationToken cancellationToken)
    {
        var teacherId = await GetActingTeacherIdAsync(cancellationToken);

        var isOwner = await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.TeacherId == teacherId, cancellationToken);

        if (!isOwner)
        {
            return NotFound();
        }

        var courseLesson = await _context.CourseLessons
            .FirstOrDefaultAsync(cl => cl.Id == courseLessonId && cl.CourseId == courseId, cancellationToken);

        if (courseLesson is null)
        {
            return NotFound();
        }

        _context.CourseLessons.Remove(courseLesson);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("lessons/catalog")]
    public async Task<ActionResult<List<Lesson>>> GetLessonsCatalog(CancellationToken cancellationToken)
    {
        var lessons = await _context.Lessons
            .AsNoTracking()
            .OrderBy(l => l.Subject)
            .ThenBy(l => l.Title)
            .ToListAsync(cancellationToken);

        return Ok(lessons);
    }

    private static CourseDto MapCourse(Course c)
    {
        return new CourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Subject = c.Subject,
            DifficultyLevel = c.DifficultyLevel,
            IsPublished = c.IsPublished,
            CreatedAt = c.CreatedAt,
            Lessons = c.CourseLessons
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

    private async Task<int> GetActingTeacherIdAsync(CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out var id) && id > 0)
        {
            return id;
        }

        var existingTeacherId = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == "Teacher" && u.IsActive)
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingTeacherId > 0)
        {
            return existingTeacherId;
        }

        var demoTeacher = new User
        {
            Name = "Teacher Demo",
            Username = "teacher_demo",
            Password = "teacher_demo",
            Role = "Teacher",
            IsApproved = true,
            IsActive = true,
            AddedOn = DateTime.UtcNow
        };

        _context.Users.Add(demoTeacher);
        await _context.SaveChangesAsync(cancellationToken);
        return demoTeacher.Id;
    }
}
