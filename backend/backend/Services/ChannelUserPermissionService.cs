using backend.Models;
using backend.Repositories.Implementations;
using backend.Repositories.Interfaces;
using System.Data;
using System.Security.Claims;

namespace backend.Services
{
    public interface IChannelUserPermissionService
    {
        Task<bool> CanRemoveMembersAsync(Guid channelId, Guid userId);
        Task<bool> CanAddMembersAsync(Guid channelId, Guid userId);
        Task<bool> CanListMembersAsync(Guid channelId, Guid userId);
        Task<Role?> GetUserRoleInChannelAsync(Guid channelId, Guid userId);
    }

    public class ChannelUserPermissionService : IChannelUserPermissionService
    {
        private readonly IChannelUserRepository _channelUserRepository;

        public ChannelUserPermissionService(IChannelUserRepository channelUserRepository)
        {
            _channelUserRepository = channelUserRepository;
            //_channelCourseRepository = ChannelCourseRepository;
        }

        public async Task<Role?> GetUserRoleInChannelAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            return channelUser?.Role;
        }

        public async Task<bool> CanAddMembersAsync(Guid channelId, Guid userId)
        {
            var role = await GetUserRoleInChannelAsync(channelId, userId);
            return role == Role.Admin;
        }

        public async Task<bool> CanRemoveMembersAsync(Guid channelId, Guid userId)
        {
            var role = await GetUserRoleInChannelAsync(channelId, userId);
            return role == Role.Admin;
        }

        public async Task<bool> CanListMembersAsync(Guid channelId, Guid userId)
        {
            var role = await GetUserRoleInChannelAsync(channelId, userId);
            return role == Role.Admin;
        }
    }
}
