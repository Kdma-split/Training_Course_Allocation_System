namespace backend.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int Age { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Channel> CreatedChannels { get; set; } = new List<Channel>();
        public ICollection<Course> CoursesInvolved { get; set; } = new List<Course>();
        public ICollection<Assignment> CourseAuthors { get; set; } = new List<Assignment>();
        public ICollection<ChannelUser> ChannelUsers { get; set; } = new List<ChannelUser>();
        public ICollection<AssignmentChannelUser> AssignmentUsers { get; set; } = new List<AssignmentChannelUser>();
    }
}
