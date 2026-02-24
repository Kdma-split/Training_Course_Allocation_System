namespace backend.Dto.Channel
{
    public class Channel
    {
        public Guid ChannelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string Admin { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<string> ChannelUsers { get; set; } = new List<string>();
        public List<string> ChannelCourses { get; set; } = new List<string>();
        public List<string> ChannelAssignments { get; set; } = new List<string>();
    }
}
