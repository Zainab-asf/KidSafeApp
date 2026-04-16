using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KidSafeApp.Backend.Data.Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, Unicode(false), MaxLength(25)]
        public string Name { get; set; }

        public DateTime AddedOn { get; set; }

        [Required, Unicode(false), MaxLength(50)]
        public string Username { get; set; }

        [Required, MaxLength(20)]
        public string Password { get; set; }

        [Required, Unicode(false), MaxLength(20)]
        public string Role { get; set; } = "Child";

        public bool IsApproved { get; set; } = false;

        public bool IsActive { get; set; } = true;


    }
}
