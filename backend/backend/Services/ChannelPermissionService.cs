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
        private readonly IChannelUserRepository _channelUserRepository;

        public ChannelPermissionService(IChannelUserRepository channelUserRepository)
        {
            _channelUserRepository = channelUserRepository;
        }

        public async Task<Models.Channel?> GetUserRoleInChannelAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            if (channelUser == null)
                return null;
            
            var channel = await _channelUserRepository.GetChannelByIdAsync(channelId);
            return channel;
        }

        public async Task<bool> CanEditChannelInfoAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            return channelUser != null;
        }
    }
}
