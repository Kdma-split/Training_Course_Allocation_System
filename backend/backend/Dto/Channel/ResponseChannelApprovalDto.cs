namespace backend.Dto.Channel
{
    public class ResponseChannelApprovalDto
    {
        public Guid ChannelApprovalId { get; set; }
        public Guid ChannelUserId { get; set; }
        public string ApprovalDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool isActived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
