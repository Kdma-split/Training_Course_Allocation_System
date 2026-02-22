namespace backend.Dto
{
    public class CreateCourseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DomainId { get; set; }
        public Guid ChannelId { get; set; }
    }

    public class UpdateCourseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DomainId { get; set; }
    }

    public class CourseResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DomainId { get; set; }
        public int NumberAttended { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
