using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.Dto.Channel
{
    public class CreateChannelDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; }
        [Required]
        [StringLength(100)]
        public string Admin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}