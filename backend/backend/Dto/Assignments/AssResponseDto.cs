using System.ComponentModel.DataAnnotations;

namespace backend.Dto.Assignments
{
    public class AssResponseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}