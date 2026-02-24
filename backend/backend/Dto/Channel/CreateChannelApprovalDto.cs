using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.Dto.Channel
{
    public class CreateChannelApprovalDto
    {
        [Required]
        [MaxLength(100)]
        public string ApprovalUserName { get; set; }
        [Required]
        [MaxLength(100)]
        public string ApprovalUserEmail{ get; set; }
        [Required]
        [Range(0, 150)]
        public int ApprovalUserAge{ get; set; }
        public Role ApprovalUserRole{ get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;    
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;    
    }
}