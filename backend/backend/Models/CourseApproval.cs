namespace backend.Models
{
    public class CourseApproval
    {
        public Guid CourseApprovalId { get; set; }
        public Guid CourseChannelUserId { get; set; }
        public string ApprovalDescription { get; set; }
        public string Status { get; set; } = "pending"; // pending, approved, rejected
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;    // indicates the creation date of the approval
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;    // indicates the date of acknowledgement of the approval
        public CourseChannelUser CourseChannelUser { get; set; }
    }
}




//THINGS TO  TTAKE CARE OF, WHILE HANDLING A PPROVAL:

//    CANNOT RE-ACKNOWLEDGE AN ALREADY ACKNOWLEDGED APPROVAL.