using backend.Models;
using backend.Repositories.Implementations;
using backend.Repositories.Interfaces;
using System.Data;
using System.Security.Claims;

namespace backend.Services
{
    public interface IChannelPermissionService
    {
        Task<bool> CanEditChannelInfoAsync(Guid channelId, Guid userId);
        Task<Channel?> GetUserRoleInChannelAsync(Guid channelId, Guid userId);
    }

    public class ChannelPermissionService : IChannelPermissionService
    {
        private readonly IChannelRepo _channelRepository;

        public ChannelPermissionService(IChannelUserRepository channelRepository)
        {
            _channelRepository = channelRepository;
            //_channelCourseRepository = ChannelCourseRepository;
        }

        public async Task<Channel?> GetUserRoleInChannelAsync(Guid channelId, Guid userId)
        {
            var channel = await _channelRepository.GetChannelByUserIdAsync(channelId, userId, "admin");
            return channel;
        }

        public async Task<bool> CanEditChannelInfoAsync(Guid channelId, Guid userId)
        {
            return GetUserRoleInChannelAsync(channelId, userId) != null;
        }
    }
}
