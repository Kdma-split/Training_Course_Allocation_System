namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface IChannelAssignmentRepository
    {
        Task<ChannelAssignment?> GetChannelAssignmentAsync(Guid channelId, Guid assignmentId);
        Task<IEnumerable<ChannelAssignment>> GetAssignmentsByChannelAsync(Guid channelId);
        Task<ChannelAssignment> AddAssignmentToChannelAsync(ChannelAssignment channelAssignment);
        Task<ChannelAssignment> UpdateChannelAssignmentAsync(ChannelAssignment channelAssignment);
        Task<bool> RemoveAssignmentFromChannelAsync(Guid channelId, Guid assignmentId);
        Task<bool> SaveChangesAsync();
    }
}
