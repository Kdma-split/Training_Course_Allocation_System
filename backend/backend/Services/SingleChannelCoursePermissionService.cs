using backend.Models;
using backend.Repositories.Implementations;
using backend.Repositories.Interfaces;
using System.Security.Claims;

namespace backend.Services
{
    public interface ISingleChannelCoursePermissionService
    {
        Task<bool> CanDeleteCourseAsync(Guid channelId, Guid userId);
        Task<bool> CanCRUDCourseAsync(Guid channelId, Guid userId);
        Task<bool> CanUpdateDeleteContentAsync(Guid channelId, Guid userId);
        Task<bool> CanReadAsync(Guid channelId, Guid userId);
        Task<Role?> GetUserRoleInChannelAsync(Guid channelId, Guid userId);
    }

    public class SingleChannelCoursePermissionService : ISingleChannelCoursePermissionService
    {
        private readonly IChannelUserRepository _channelUserRepository;

        public SingleChannelCoursePermissionService(IChannelUserRepository channelUserRepository)
        {
            _channelUserRepository = channelUserRepository;
            //_channelCourseRepository = ChannelCourseRepository;
        }

        public async Task<Role?> GetUserRoleInChannelAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            return channelUser?.Role;
        }

        public async Task<bool> CanDeleteCourseAsync(Guid channelId, Guid userId)
        {
            var role = await GetUserRoleInChannelAsync(channelId, userId);
            return role == Role.Admin;
        }

        public async Task<bool> CanCRUDCourseAsync(Guid channelId, Guid userId)
        {
            var role = await GetUserRoleInChannelAsync(channelId, userId);
            return role == Role.Author || role == Role.Admin;
        }

        public async Task<bool> CanUpdateDeleteContentAsync(Guid channelId, Guid userId)
        {
            var role = await GetUserRoleInChannelAsync(channelId, userId);
            return role == Role.Editor || role == Role.Author || role == Role.Admin;
        }

        public async Task<bool> CanReadAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, userId);
            return channelUser != null;
        }
    }
}
