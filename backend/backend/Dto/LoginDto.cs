using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [Range(18, 150)]
        public int Age { get; set; }  // Changed from 'age' to PascalCase

        // public string? ProfilePicture { get; set; }
    }
}


// AUTHOR CAN ONLY HAVE EDIT ACESS ON HIS CHANNEL(S) AND NOT ON ANY OTHERS...
