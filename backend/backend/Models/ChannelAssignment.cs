namespace backend.Models
{
    public class ChannelAssignment
    {
        public Guid ChannelAssignmentId { get; set; } = Guid.NewGuid();
        public Guid? CourseId { get; set; }     //  this allows to create an assignment independently without a creating a course (null --- assignment can be created independently without a course)

        public Guid AssignmentId { get; set; }

        public Guid ChannelId { get; set; }

        public bool IsActive { get; set; } = true;  // determines whether the course is active or not at a channel level... (soft delete)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Course Course { get; set; }
        public Channel Channel { get; set; }
        public Assignment Assignment { get; set; }
    }
}












//namespace backend.Models
//{
//    class CourseAssignment
//    {
//        public Guid CourseAssignmentId { get; set; }

//        // Foreign key to the Course
//        public Guid? CourseId { get; set; }  // allows to create an asignment independently without a creating a course
//        public Guid AssignmentId { get; set; }
//        public Guid ChannelId { get; set; }  // the user belong to which channel (null --- user has to be in a channel to contribute)
//        public bool IsActive { get; set; }  // determines whether the course is deleted or not... (soft delete)
//        public DateTime Created { get; set; }
//        public DateTime LastUpdatedAt { get; set; }

//        // Foreign key to the User (Author)

//        // Navigation properties
//        public Course Course { get; set; }   // one assignment can have only one course
//        public IEnumerable<User> Authors { get; set; }   // one assignment can have multiple authors (editors/viewers)
//        public IEnumerable<Channel> Channels { get; set; } // one assignment can be associated with multiple channels (if needed)
//    }
//}