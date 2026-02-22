namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface IChannelUserRepository
    {
        Task<ChannelUser?> GetChannelUserAsync(Guid channelId, Guid userId);
        Task<IEnumerable<ChannelUser>> GetUsersByChannelAsync(Guid channelId);
        Task<ChannelUser> AddUserToChannelAsync(ChannelUser channelUser);
        Task<ChannelUser> UpdateUserRoleAsync(ChannelUser channelUser);
        Task<bool> RemoveUserFromChannelAsync(Guid channelId, Guid userId);
        Task<bool> SaveChangesAsync();
        Task<Channel?> GetChannelByIdAsync(Guid channelId);
    }
}
