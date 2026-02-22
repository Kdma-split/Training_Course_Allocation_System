using System.ComponentModel.DataAnnotations;

namespace backend.Dto.Assignments
{
    public class CreateAssDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
        [Required]
        [StringLength(100)]
        public string Domain { get; set; }  
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}