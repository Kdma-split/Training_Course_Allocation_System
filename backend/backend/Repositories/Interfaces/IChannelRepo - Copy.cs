using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IChannelRepo
    {
        Task<IEnumerable<Channel>> GetAllChannelsAsync();
        Task<Channel> GetChannelByIdAsync(Guid id);
        Task<IEnumerable<Channel?>?> GetChnannelByUserId(Guid id, string userType);
        Task<Channel> CreateChannelAsync(Channel channel);
        Task<Channel> UpdateChannelAsync(Guid id, Channel channel);
        Task<bool> DeleteChannelAsync(Guid id);
        Task<bool> IsChannelExits(string name);
        Task<IEnumerable<ChannelUser>> GetChannelUsersAsync(Guid channelId);
    }
}