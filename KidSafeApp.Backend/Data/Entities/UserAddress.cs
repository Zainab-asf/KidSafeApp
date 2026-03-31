using System.ComponentModel.DataAnnotations;

namespace KidSafeApp.Backend.Data.Entities
{
    public class UserAddress
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
       
        [Required, MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }


}