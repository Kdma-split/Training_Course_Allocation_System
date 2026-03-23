using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Interfaces
{
    public interface IChannelUserRoleRepository
    {
        Task<IEnumerable<string>?> GetUserRoleInChannelAsync(Guid channelId, Guid userId);
        Task<bool> IsUserRoleExists(Guid userId, Guid channelId, string roleName);
        Task<IEnumerable<User>?> GetUsersByRole(Guid channelId, string roleName);
        Task<IEnumerable<string>> GetUserRoles(Guid userId, Guid channelId);
        Task AddUserRoleAsync(Guid UserId, IEnumerable<int> roleIds);
        Task RemoveUserRoleAsync(Guid channelUserRoleId);
        Task UpdateUserRoleAsync(ChannelUserRoles channelUserRole);

        // Direct DB operations - business logic handled in controllers
        Task<IEnumerable<string>> GetUserRolesByChannelUserIdAsync(Guid channelUserId);
        Task AddRoleByChannelUserIdAsync(Guid channelUserId, int roleId);
        Task AddRolesBulkByChannelUserIdAsync(Guid channelUserId, IEnumerable<int> roleIds);
        Task RemoveRoleByChannelUserRoleIdAsync(Guid channelUserRoleId);
        Task RemoveRolesByChannelUserIdAsync(Guid channelUserId);
        Task UpdateRoleByChannelUserRoleIdAsync(Guid channelUserRoleId, int newRoleId);
        Task UpdateRolesByChannelUserIdAsync(Guid channelUserId, IEnumerable<int> newRoleIds);
        Task<bool> GetMemberOrAdminAsync(Guid channelId, Guid userId);
    }
}
