namespace backend.Models
{
    public class ChannelUser
    {
        public Guid ChannelUserId { get; set; } = Guid.NewGuid();
        public Guid ChannelId { get; set; }
        public Channel Channel { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Role Role { get; set; }
        public bool isActive { get; set; } = true;  // determines whether the user is removed from the channel or not... (soft delete)

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; }    // updated when the user is removed from the channel (isActive = false) or when the user's role is updated (e.g., from viewer to editor, etc.)
    }
}
