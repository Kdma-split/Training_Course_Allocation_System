namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface IChannelRepository
    {
        Task<Channel?> GetChannelByIdAsync(Guid id);
        Task<IEnumerable<Channel>> GetAllChannelsAsync();
        Task<Channel> CreateChannelAsync(Channel channel);
        Task<Channel> UpdateChannelAsync(Channel channel);
        Task<bool> DeleteChannelAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
