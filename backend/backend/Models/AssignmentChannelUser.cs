namespace backend.Models
{
    class AssignmentChannelUser
    {
        public int AssignmentUserId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ChannelAssignmentId { get; set; }  // the user belong to which channel (null --- user has to be in a channel to contribute)
        public string Role { get; set; } // e.g., "Editor", "Viewer", "Author"
        public bool IsActive { get; set; } = true;  // determines whether the user from the particular course is removed or not... (soft delete) [signifies his/her discontinuation from making the assignment further...]
        public Status status { get; set; } = Status.Pending; // e.g., "Pending", "Completed"   // can be used to track minute details like: whether the user has started working on the assignment or not, whether the user has completed the assignment or not, etc. (can be used for analytics and insights)
        public string? task { get; set; } // e.g., "Video Upload", "Course Creation", "Assignment Creation", etc. (can be used for analytics and insights to track which type of task is being performed more by the users, which type of task is being performed more in which channel, etc.)
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


// THIS DESIGN OF ASSIGNMENTS MODEL ALLOWS TO CREATE ASSIGNMENTS UNDER A COURSE (CourseId not null) OR EVEN INDEPEDENTLY WITHOUT A COURSE...
