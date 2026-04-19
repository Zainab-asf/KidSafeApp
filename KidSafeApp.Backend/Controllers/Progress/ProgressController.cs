using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KidSafeApp.Backend.Controllers.Progress
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly DataContext _context;

        public ProgressController(DataContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // GET: api/progress
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProgressDto>>> GetUserProgress()
        {
            var userId = GetCurrentUserId();
            var progress = await _context.UserProgress
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.LastAccessedDate)
                .Select(p => new UserProgressDto
                {
                    Id = p.Id,
                    CourseTitle = p.CourseTitle,
                    LessonsCompleted = p.LessonsCompleted,
                    TotalLessons = p.TotalLessons,
                    PercentageComplete = p.PercentageComplete,
                    StartDate = p.StartDate,
                    CompletionDate = p.CompletionDate,
                    Status = p.Status,
                    Streak = p.Streak,
                    TotalPoints = p.TotalPoints,
                    LastAccessedDate = p.LastAccessedDate
                })
                .ToListAsync();

            return Ok(progress);
        }

        // GET: api/progress/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProgressDto>> GetProgress(int id)
        {
            var userId = GetCurrentUserId();
            var progress = await _context.UserProgress
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (progress == null)
                return NotFound();

            return Ok(new UserProgressDto
            {
                Id = progress.Id,
                CourseTitle = progress.CourseTitle,
                LessonsCompleted = progress.LessonsCompleted,
                TotalLessons = progress.TotalLessons,
                PercentageComplete = progress.PercentageComplete,
                StartDate = progress.StartDate,
                CompletionDate = progress.CompletionDate,
                Status = progress.Status,
                Streak = progress.Streak,
                TotalPoints = progress.TotalPoints,
                LastAccessedDate = progress.LastAccessedDate
            });
        }

        // POST: api/progress
        [HttpPost]
        public async Task<ActionResult<UserProgressDto>> CreateProgress(UserProgressDto dto)
        {
            var userId = GetCurrentUserId();

            var progress = new UserProgress
            {
                UserId = userId,
                CourseTitle = dto.CourseTitle,
                LessonsCompleted = 0,
                TotalLessons = dto.TotalLessons,
                PercentageComplete = 0,
                StartDate = DateTime.UtcNow,
                Status = "In Progress",
                Streak = 0,
                TotalPoints = 0,
                LastAccessedDate = DateTime.UtcNow
            };

            _context.UserProgress.Add(progress);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProgress", new { id = progress.Id }, new UserProgressDto
            {
                Id = progress.Id,
                CourseTitle = progress.CourseTitle,
                LessonsCompleted = progress.LessonsCompleted,
                TotalLessons = progress.TotalLessons,
                PercentageComplete = progress.PercentageComplete,
                StartDate = progress.StartDate,
                CompletionDate = progress.CompletionDate,
                Status = progress.Status,
                Streak = progress.Streak,
                TotalPoints = progress.TotalPoints,
                LastAccessedDate = progress.LastAccessedDate
            });
        }

        // PUT: api/progress/5/update-lesson
        [HttpPut("{id}/update-lesson")]
        public async Task<IActionResult> UpdateLessonProgress(int id, [FromBody] UpdateLessonProgressRequest request)
        {
            var userId = GetCurrentUserId();
            var progress = await _context.UserProgress
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (progress == null)
                return NotFound();

            progress.LessonsCompleted = Math.Min(request.LessonNumber, progress.TotalLessons);
            progress.PercentageComplete = (int)((progress.LessonsCompleted * 100) / progress.TotalLessons);
            progress.LastAccessedDate = DateTime.UtcNow;

            // Award points
            progress.TotalPoints += request.PointsEarned;

            // Update streak if completed today
            if (progress.LastAccessedDate.Date == DateTime.UtcNow.Date)
            {
                progress.Streak++;
            }

            // Mark as completed if all lessons done
            if (progress.LessonsCompleted >= progress.TotalLessons)
            {
                progress.Status = "Completed";
                progress.CompletionDate = DateTime.UtcNow;
            }

            progress.UpdatedAt = DateTime.UtcNow;

            _context.UserProgress.Update(progress);
            await _context.SaveChangesAsync();

            return Ok(progress);
        }

        // GET: api/progress/summary
        [HttpGet("summary/stats")]
        public async Task<ActionResult<ProgressSummaryDto>> GetProgressSummary()
        {
            var userId = GetCurrentUserId();
            var allProgress = await _context.UserProgress
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var summary = new ProgressSummaryDto
            {
                TotalCourses = allProgress.Count,
                CompletedCourses = allProgress.Count(p => p.Status == "Completed"),
                InProgressCourses = allProgress.Count(p => p.Status == "In Progress"),
                TotalPoints = allProgress.Sum(p => p.TotalPoints),
                AverageStreak = allProgress.Count > 0 ? allProgress.Average(p => p.Streak) : 0,
                LongestStreak = allProgress.Count > 0 ? allProgress.Max(p => p.Streak) : 0,
                TotalLessonsCompleted = allProgress.Sum(p => p.LessonsCompleted)
            };

            return Ok(summary);
        }
    }

    public class UpdateLessonProgressRequest
    {
        public int LessonNumber { get; set; }
        public int PointsEarned { get; set; } = 10;
    }

    public class ProgressSummaryDto
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int TotalPoints { get; set; }
        public double AverageStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalLessonsCompleted { get; set; }
    }
}
