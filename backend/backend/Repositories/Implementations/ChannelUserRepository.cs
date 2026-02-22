namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class ChannelUserRepository : IChannelUserRepository
    {
        private readonly TrainingCourseContext _context;

        public ChannelUserRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<ChannelUser?> GetChannelUserAsync(Guid channelId, Guid userId)
        {
            return await _context.ChannelUsers
                .FirstOrDefaultAsync(cu => cu.ChannelId == channelId && cu.UserId == userId);
        }

        public async Task<IEnumerable<ChannelUser>> GetUsersByChannelAsync(Guid channelId)
        {
            return await _context.ChannelUsers
                .Where(cu => cu.ChannelId == channelId)
                .ToListAsync();
        }

        public async Task<ChannelUser> AddUserToChannelAsync(ChannelUser channelUser)
        {
            _context.ChannelUsers.Add(channelUser);
            await _context.SaveChangesAsync();
            return channelUser;
        }

        public async Task<ChannelUser> UpdateUserRoleAsync(ChannelUser channelUser)
        {
            _context.ChannelUsers.Update(channelUser);
            await _context.SaveChangesAsync();
            return channelUser;
        }

        public async Task<bool> RemoveUserFromChannelAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _context.ChannelUsers
                .FirstOrDefaultAsync(cu => cu.ChannelId == channelId && cu.UserId == userId);
            
            if (channelUser == null)
                return false;

            _context.ChannelUsers.Remove(channelUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Channel?> GetChannelByIdAsync(Guid channelId)
        {
            return await _context.Channels.FindAsync(channelId);
        }
    }
}
