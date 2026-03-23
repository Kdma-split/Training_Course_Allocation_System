namespace backend.Models
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }

        public int NumberAttended { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChannelCourse> ChannelCourses { get; set; } = new List<ChannelCourse>();
        public ICollection<ChannelAssignment> ChannelAssignments { get; set; } = new List<ChannelAssignment>();
    }
}
