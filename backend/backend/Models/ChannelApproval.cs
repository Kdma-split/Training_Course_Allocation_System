namespace backend.Models
{
    public class ChannelApproval
    {
        public Guid ChannelApprovalId { get; set; }
        public Guid ChannelUserId { get; set; }
        public string ApprovalDescription { get; set; }
        public string Status { get; set; } = "pending"; // pending, approved, rejected
        public bool isActived { get; set; }  // signifies, whether the approval is cancelled by the user or not... user can only change this, not the status, status can only be changed by the admin, not by any author, editor, normal users...
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;    // indicates the creation date of the approval
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;    // indicates the date of acknowledgement of the approval
        public ChannelUser ChannelUser { get; set; }
    }
}




//THINGS TO  TTAKE CARE OF, WHILE HANDLING A PPROVAL:

//    CANNOT RE-ACKNOWLEDGE AN ALREADY ACKNOWLEDGED APPROVAL.