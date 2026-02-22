namespace backend.Dto
{
    public class CreateAssignmentDto
    {
        public string Domain { get; set; } = string.Empty;
        public Guid? CourseId { get; set; }
        public Guid ChannelId { get; set; }
    }

    public class UpdateAssignmentDto
    {
        public Guid Id { get; set; }
        public string Domain { get; set; } = string.Empty;
    }

    public class AssignmentResponseDto
    {
        public Guid Id { get; set; }
        public string Domain { get; set; } = string.Empty;
        public int NumberAttended { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
