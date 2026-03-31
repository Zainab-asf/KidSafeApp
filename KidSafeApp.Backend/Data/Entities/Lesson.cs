using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Backend.Data.Entities
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ThumbnailUrl { get; set; }

        [MaxLength(50)]
        public string? Subject { get; set; }

        public int DifficultyLevel { get; set; }

        public static Lesson[] GetSeedData()
        {
            Lesson[] lessons =
            [
                new() { Id = 1, Title = "Online Safety Basics", Subject = "Safety", DifficultyLevel = 1 },
                new() { Id = 2, Title = "Cyberbullying Awareness", Subject = "Safety", DifficultyLevel = 2 },
                new() { Id = 3, Title = "Respectful Communication", Subject = "Social", DifficultyLevel = 1 }
            ];

            return lessons;
        }
    }
}
