namespace backend.Models
{
    public class Assignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Domain { get; set; }
        public int NumberAttended { get; set; }     // helps to infer props like the most popular course assignment on the entire platform -- not on a channel (a course assignment can be present on many channels)
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChannelAssignment> ChannelAssignments { get; set; } = new List<ChannelAssignment>();
        // public ICollection<AssignmentChannelUser> ChannelAssignmentUsers { get; set; } = new List<AssignmentChannelUser>();
    }
}



// MULTIPLE SAME ASSIGNMENTS CAN HAVE A SIMILAR NAME BUT NOT THE SAME ---- UNNECCESSARY RECORDS ESCALATION, HOW TO TACKLE THIS ISSUE --- AT THE SAME TIME YOU HAVE TO PRESERVE THE SAME NAME GIVEN BY THE AUTHOR AS YOU HAVE TO RETURN THE SAME TO THE FRONTEND ---- ONE WAY OF DOING THIS IS TO USE FIXED OPTIONS BUTTON ON THE FRONTEND...
