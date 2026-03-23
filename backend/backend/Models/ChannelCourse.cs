namespace backend.Models
{
    public class ChannelCourse
    {
        public Guid ChannelCourseId { get; set; }
        public Guid ChannelId { get; set; }

        public Guid CourseId { get; set; }

        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "pending"; // pending, completed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Channel Channel { get; set; }
        public Course Course { get; set; }
    }
}


// SIGNIFICANCE OF "status" FIELD:


// AUTHOR --- CREATORS OF A COURSE ... 
// videos are being uploaded and assigned to a course, so the author of the course should have edit access to the course and its content (videos)...


// VIEWER --- NORMAL USERS
// viewrs can start a course, status is "pending", and once all the videos are completed, status = "completed"...






    // CREATE MULTIPLE SUBSCRIPTIONS INSTEAD OF MULTIPLE RESOURCE GROUPS
    // DIFFERENCE BETWEEN RBAC AND RESOURCE GROUPS