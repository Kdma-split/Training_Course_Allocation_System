using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Channel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        public string Description { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public Guid AdminId { get; set; }
        public User Admin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChannelUser> ChannelUsers { get; set; } = new List<ChannelUser>();
        public ICollection<ChannelCourse> ChannelCourses { get; set; } = new List<ChannelCourse>();
        public ICollection<ChannelAssignment> ChannelAssignments { get; set; } = new List<ChannelAssignment>();
    }
}