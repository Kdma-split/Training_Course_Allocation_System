namespace backend.Models
{
    public enum Status
    {
        Pending = 1,
        Completed = 2,
        Removed = 3,
    }
    
    //public class ChannelApprovalDetails
    //{
    //    public string Name { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty;
    //    public string Role { get; set; } = string.Empty;
    //    public int Age { get; set; }
    //}

    //public class UpdateChannelApprovalStatus
    //{
    //    public Guid ApprovalId { get; set; }
    //    public string Status { get; set; } = string.Empty;

    //    public UpdateChannelApprovalStatus(Guid approvalId, string status)
    //    {
    //        ApprovalId = approvalId;
    //        Status = status;
    //    }
    //}
}
