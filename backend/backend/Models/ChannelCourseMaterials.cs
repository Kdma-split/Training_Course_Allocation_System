namespace backend.Models
{
    public class ChannelCourseMaterials
    {
        public Guid ChannelCourseId { get; set; }
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;    // instead of directly deleting the course twitch this option, so that we can restore the course again if required in future, and we can also create a stored procedure that can be run in every 30 days that deletes the media materials that are not active for more than 30 days, this will help us to keep the database clean and also we can restore the course if required in future without losing the media materials...
        public string Status { get; set; } = "pending"; // pending, completed
        public string MediaUrl { get; set; } = string.Empty;
        public Guid ChannelAssignemntId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ChannelCourse Course { get; set; }
    }
}