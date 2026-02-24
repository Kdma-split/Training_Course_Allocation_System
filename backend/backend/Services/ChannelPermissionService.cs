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
        Task<bool> CanEditApprovalAsync(Guid channelId, Guid userId);
        //Task<bool> CanCreateApprovalAsync(Guid channelId, Guid userId);
        Task<Role?> GetUserRoleInChannelAsync(Guid channelId, Guid userId);
    }

    public class ChannelPermissionService : IChannelPermissionService
    {
        private readonly IChannelUserRepository _channelUserRepository;

        public ChannelPermissionService(IChannelUserRepository channelUserRepository)
        {
            _channelUserRepository = channelUserRepository;
        }

        //public async Task<Models.Channel?> GetUserRoleInChannelAsync(Guid channelId, Guid userId)
        public async Task<Role?> GetUserRoleInChannelAsync(Guid channelId, Guid userId)

        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            //if (channelUser == null)
            //    return null;

            //var channel = await _channelUserRepository.GetChannelByIdAsync(channelId);
            //return channel;
            return channelUser?.Role;
        }

        public async Task<bool> CanEditChannelInfoAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            return channelUser?.Role == Role.Admin;
        }

        public async Task<bool> CanEditApprovalAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            return channelUser?.Role == Role.Admin;
        }
    }
}
