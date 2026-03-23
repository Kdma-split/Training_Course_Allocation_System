namespace backend.Dto
{
    public class CreateCourseApprovalDto
    {
        public string ApprovalDescription { get; set; } = string.Empty;
    }

    public class CourseApprovalResponseDto
    {
        public Guid CourseApprovalId { get; set; }
        public Guid UserId { get; set; }
        public string ApprovalDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ApprovalDescriptionDto
    {
        public string Category { get; set; } = string.Empty;
        public Guid ChannelId { get; set; }
        public ApprovalDetailsDto Details { get; set; } = new();
    }

    public class ApprovalDetailsDto
    {
        public Guid? CourseId { get; set; }
    }
}
