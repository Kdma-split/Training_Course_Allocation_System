namespace backend.Models
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        public int DomainId { get; set; }   
        public Domain Domain { get; set; }

        public int NumberAttended { get; set; }      // helps to infer props like the most popular course on the entire platform -- not on a channel (a course can e present on many platforms)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChannelCourse> ChannelCourses { get; set; } = new List<ChannelCourse>();
        public ICollection<ChannelAssignment> CourseAssignments { get; set; } = new List<ChannelAssignment>();
    }
}










using System.Threading.Channels;

namespace backend.Models
{
    class Course
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DomainId { get; set; }

        // Foreign key to the Channel
        public int NumberAttended { get; set; } // number of users who have attended the courses

        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        // Navigation property to the Channel
        public IEnumerable<User> Authors { get; set; }

        //public IEnumerable<Channel> Channels { get; set; }

        // ISSUE: ✗ INCORRECT - This is backwards
        // Justification: Course has ChannelId (many courses belong to ONE channel)
        // Should be: public Channel Channel { get; set; } (many-to-one, single Channel)
        // Current property suggests one course has many channels, which contradicts the foreign key design
        // Recommend: public Channel Channel { get; set; }

        //SOLUTION: Make a separate table "ChannelCourse"

        public IEnumerable<Channel> Channel { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}