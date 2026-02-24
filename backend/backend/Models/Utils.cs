namespace backend.Models
{
    public enum Role
    {
        Viewer = 1,
        Editor = 2,
        Author = 3,
        Admin = 4
    }

    public enum Status
    {
        Pending = 1,
        Completed = 2,
        Removed = 3,
    }
    
    public class ChannelApprovalDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; }
        public int Age { get; set; }
    }

    public class UpdateChannelApprovalStatus
    {
        public Guid ApprovalId { get; set; }
        public string Status { get; set; } = string.Empty;

        public UpdateChannelApprovalStatus(Guid approvalId, string status)
        {
            ApprovalId = approvalId;
            Status = status;
        }
    }
}
