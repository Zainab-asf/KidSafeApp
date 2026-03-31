using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Shared.DTOs
{
    public class LoginDto
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(20), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
