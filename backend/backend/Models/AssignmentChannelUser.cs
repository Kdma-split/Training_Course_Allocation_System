namespace backend.Models
{
    public class AssignmentChannelUser
    {
        public int AssignmentChannelUserId { get; set; }
        public Guid UserId { get; set; }
        public Guid ChannelAssignmentId { get; set; }     // the user belong to which channel (null --- user has to be in a channel to contribute)
        public Role Role { get; set; }    // e.g., "Editor", "Viewer", "Author"
        public bool IsActive { get; set; } = true;     // determines whether the user from the particular course is removed or not... (soft delete) [signifies his/her discontinuation from making the assignment further...]
        public Status Status { get; set; } = Status.Pending;     // e.g., "Pending", "Completed"   // can be used to track minute details like: whether the user has started working on the assignment or not, whether the user has completed the assignment or not, etc. (can be used for analytics and insights)
        public string? Task { get; set; }     // e.g., "Video Upload", "Course Creation", "Assignment Creation", etc. (can be used for analytics and insights to track which type of task is being performed more by the users, which type of task is being performed more in which channel, etc.)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public ChannelAssignment ChannelAssignment { get; set; }
        public User User { get; set; }
    }
}

