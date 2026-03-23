namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class ChannelUserRoleRepository : IChannelUserRoleRepository
    {
        private readonly TrainingCourseContext _context;

        public ChannelUserRoleRepository(TrainingCourseContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<string>?> GetUserRoleInChannelAsync(Guid channelId, Guid userId)
        {
            var roleNames = await (from cu in _context.ChannelUsers
                                   join cur in _context.ChannelUserRoles on cu.ChannelUserId equals cur.ChannelUserId
                                   join r in _context.Roles on cur.RoleId equals r.Id
                                   where cu.ChannelId == channelId && cu.UserId == userId
                                   select r.Name).ToListAsync();

            return roleNames.Any() ? roleNames : null;
        }

        public async Task<bool> IsUserRoleExists(Guid userId, Guid channelId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return false;

            return await _context.ChannelUserRoles
                .AnyAsync(cur => cur.ChannelUser.UserId == userId 
                               && cur.ChannelUser.ChannelId == channelId 
                               && cur.RoleId == role.Id);
        }

        public async Task<IEnumerable<User>?> GetUsersByRole(Guid channelId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return null;

            var users = await (from cu in _context.ChannelUsers
                              join cur in _context.ChannelUserRoles on cu.ChannelUserId equals cur.ChannelUserId
                              join r in _context.Roles on cur.RoleId equals r.Id
                              where cu.ChannelId == channelId && r.Name == roleName
                              select cu.User).ToListAsync();

            return users;
        }

        public async Task<IEnumerable<string>> GetUserRoles(Guid userId, Guid channelId)
        {
            var roleNames = await (from cu in _context.ChannelUsers
                                  join cur in _context.ChannelUserRoles on cu.ChannelUserId equals cur.ChannelUserId
                                  join r in _context.Roles on cur.RoleId equals r.Id
                                  where cu.UserId == userId && cu.ChannelId == channelId
                                  select r.Name).ToListAsync();

            return roleNames;
        }

        public async Task<IEnumerable<string>> GetUserRolesByChannelUserIdAsync(Guid channelUserId)
        {
            var roleNames = await (from cur in _context.ChannelUserRoles
                                   join r in _context.Roles on cur.RoleId equals r.Id
                                   where cur.ChannelUserId == channelUserId
                                   select r.Name).ToListAsync();

            return roleNames;
        }

        public async Task AddUserRoleAsync(Guid userId, IEnumerable<int> roleIds)
        {
            var channelUser = await _context.ChannelUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId);

            if (channelUser == null) return;

            foreach (var roleId in roleIds)
            {
                var channelUserRole = new ChannelUserRoles
                {
                    Id = Guid.NewGuid(),
                    ChannelUserId = channelUser.ChannelUserId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ChannelUserRoles.Add(channelUserRole);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddRolesBulkByChannelUserIdAsync(Guid channelUserId, IEnumerable<int> roleIds)
        {
            foreach (var roleId in roleIds)
            {
                var channelUserRole = new ChannelUserRoles
                {
                    Id = Guid.NewGuid(),
                    ChannelUserId = channelUserId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ChannelUserRoles.Add(channelUserRole);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddRoleByChannelUserIdAsync(Guid channelUserId, int roleId)
        {
            var channelUserRole = new ChannelUserRoles
            {
                Id = Guid.NewGuid(),
                ChannelUserId = channelUserId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.ChannelUserRoles.Add(channelUserRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserRoleAsync(Guid channelUserRoleId)
        {
            var channelUserRole = await _context.ChannelUserRoles.FindAsync(channelUserRoleId);
            if (channelUserRole != null)
            {
                _context.ChannelUserRoles.Remove(channelUserRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveRoleByChannelUserRoleIdAsync(Guid channelUserRoleId)
        {
            var channelUserRole = await _context.ChannelUserRoles.FindAsync(channelUserRoleId);
            _context.ChannelUserRoles.Remove(channelUserRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRolesByChannelUserIdAsync(Guid channelUserId)
        {
            var rolesToRemove = await _context.ChannelUserRoles
                .Where(cur => cur.ChannelUserId == channelUserId)
                .ToListAsync();

            _context.ChannelUserRoles.RemoveRange(rolesToRemove);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserRoleAsync(ChannelUserRoles channelUserRole)
        {
            _context.ChannelUserRoles.Update(channelUserRole);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoleByChannelUserRoleIdAsync(Guid channelUserRoleId, int newRoleId)
        {
            var channelUserRole = await _context.ChannelUserRoles.FindAsync(channelUserRoleId);
            if (channelUserRole == null) return;

            channelUserRole.RoleId = newRoleId;
            channelUserRole.UpdatedAt = DateTime.UtcNow;
            
            _context.ChannelUserRoles.Update(channelUserRole);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRolesByChannelUserIdAsync(Guid channelUserId, IEnumerable<int> newRoleIds)
        {
            var existingRoles = await _context.ChannelUserRoles
                .Where(cur => cur.ChannelUserId == channelUserId)
                .ToListAsync();

            _context.ChannelUserRoles.RemoveRange(existingRoles);

            var newRoles = newRoleIds.Select(roleId => new ChannelUserRoles
            {
                Id = Guid.NewGuid(),
                ChannelUserId = channelUserId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.ChannelUserRoles.AddRangeAsync(newRoles);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetMemberOrAdminAsync(Guid channelId, Guid userId)
        {
            var roles = await GetUserRoleInChannelAsync(channelId, userId);
            return roles != null;
        }
    }
}





