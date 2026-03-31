using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(25)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(20), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
