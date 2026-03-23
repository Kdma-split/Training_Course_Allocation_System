namespace backend.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class ChannelApproval
    {
        public Guid ChannelApprovalId { get; set; }
        public Guid ChannelUserId { get; set; }
        public string ApprovalDescription { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";    // STATES: pending, rejected, approved
        public bool IsActive { get; set; }
        public Guid? ApprovedBy { get; set; }

        [ForeignKey("ApprovedBy")]
        public ChannelUser ApprovedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ChannelUser ChannelUser { get; set; }
    }
}
