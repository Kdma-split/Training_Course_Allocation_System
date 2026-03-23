namespace backend.Models
{
    public class ChannelUserRoles
    {
        public Guid Id { get; set; }
        public Guid ChannelUserId { get; set; }
        public ChannelUser ChannelUser { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}

// A USER CAN HAVE MULTIPLE ROLES IN A CHANNEL --- PROVIDED HE'S NOT THE ADMIN. A CATCH HERE, ONLY THE ONE WHO HAS CREATED THE CHANNEL CAN HAVE OTHER ROLES ALONG WITH BEING THE ADMIN. 
// A CHANNEL CAN HAVE MULTIPLE ADMINS. ONLY ADMINS CAN ONLY ASSIGN OTHER ADMINS.