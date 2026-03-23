namespace backend.Dto.Channel
{
    public class ResponseChannelApprovalDto
    {
        public Guid ChannelApprovalId { get; set; }
        public Guid ChannelUserId { get; set; }
        public ChannelApprovalUserDto? ApprovalDescription { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool isActived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
