namespace backend.Models
{
    public class Role
    {
        public int Id { get; set; } 
        public  string Name { get; set; }    // e.g., ChannelAdmin, ChannelEditor, ChannelViewer, CourseAdmin, CourseEditor, CourseViewer, AssignmentAdmin, AssignmentEditor, AssignmentViewer .
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}