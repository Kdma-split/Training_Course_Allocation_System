namespace backend.Models
{
    public class ChannelCourse
    {
        public Guid ChannelCourseId { get; set; }
        public Guid ChannelId { get; set; }

        public Guid CourseId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Channel Channel { get; set; }
        public Course Course { get; set; }
    }
}

















namespace backend.Models
{
    class ChannelCourse
    {
        public int CourseChannelId { get; set; }
        public Guid ChannelId { get; set; }
        public Guid CourseId { get; set; } 
        public string Role { get; set; } // e.g., "Editor", "Viewer", "Author"
        public bool IsActive { get; set; }  // determines whether the course is deleted or not... (soft delete)
        public string status { get; set; } // e.g., "Pending", "Completed"
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IEnumerable<User> users { get; set; }
}

// SIGNIFICANCE OF "status" FIELD:


// AUTHOR --- CREATORS OF A COURSE ... 
// videos are being uploaded and assigned to a course, so the author of the course should have edit access to the course and its content (videos)...


// VIEWER --- NORMAL USERS
// viewrs can start a course, status is "pending", and once all the videos are completed, status = "completed"...






    // CREATE MULTIPLE SUBSCRIPTIONS INSTEAD OF MULTIPLE RESOURCE GROUPS
    // DIFFERENCE BETWEEN RBAC AND RESOURCE GROUPS