using System.ComponentModel.DataAnnotations;

namespace backend.Dto.Auth
{
    public class LoginUser
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [StringLength(255)]
        public string Password { get; set; }
    }
}


// AUTHOR CAN ONLY HAVE EDIT ACESS ON HIS CHANNEL(S) AND NOT ON ANY OTHERS...
