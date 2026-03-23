using backend.Models;

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
        Task<bool> SoftDeleteChannelAsync(Guid id);
        Task<bool> SaveChangesAsync();
        Task<IEnumerable<Channel?>?> GetChannelsByUserIdAsync(Guid id, string userType);
        Task<Channel?> GetChannelByUserIdAsync(Guid channelId, Guid userId, string userType);
        Task<bool> IsChannelExists(Guid id);
    }
}


//using backend.Models;

//namespace backend.Repositories.Interfaces
//{
//    public interface IChannelRepo
//    {
//        Task<IEnumerable<Channel>> GetAllChannelsAsync();
//        Task<Channel> GetChannelByIdAsync(Guid id);
//        Task<IEnumerable<Channel?>?> GetChannelsByUserIdAsync(Guid id, string userType);
//        Task<Channel?> GetChannelByUserIdAsync(Guid channelId, Guid userId, string userType);
//        Task<Channel> CreateChannelAsync(Channel channel);
//        Task<Channel> UpdateChannelAsync(Guid id, Channel channel);
//        Task<bool> DeleteChannelAsync(Guid id);
//        Task<bool> IsChannelExits(string name);
//        Task<IEnumerable<ChannelUser>> GetChannelUsersAsync(Guid channelId);
//    }
//}