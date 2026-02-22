using backend.Models;

namespace backend.Dto.Channel
{
    public class Channel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string Admin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<ChannelUser> ChannelUsers { get; set; }
        public List<ChannelCourse> ChannelCourses { get; set; }
        public List<ChannelAssignment> ChannelAssignments { get; set; }
    }
}