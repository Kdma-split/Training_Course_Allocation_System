namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Identity.Client;

    public class ChannelRepository : IChannelRepository
    {
        private readonly TrainingCourseContext _context;

        public ChannelRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<bool> IsChannelExists(string name)
        {
            return await _context.Channels.AnyAsync(c => c.Name == name);
        }

        public async Task<Channel?> GetChannelByIdAsync(Guid id)
        {
            return await _context.Channels.FindAsync(id);
        }

        public async Task<IEnumerable<Channel>> GetAllChannelsAsync()
        {
            //return await _context.Channels.AsNoTracking().ToListAsync();
            return await _context.Channels.ToListAsync();
        }

        public async Task<Channel> CreateChannelAsync(Channel channel)
        {
            _context.Channels.Add(channel);
            await _context.SaveChangesAsync();
            return channel;
        }

        public async Task<Channel> UpdateChannelAsync(Channel channel)
        {
            _context.Channels.Update(channel);
            await _context.SaveChangesAsync();
            return channel;
        }

        public async Task<bool> DeleteChannelAsync(Guid id)
        {
            var channel = await _context.Channels.FindAsync(id);
            if (channel == null)
                return false;

            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteChannelAsync(Guid id)
        {
            var channel = await _context.Channels
                .Include(c => c.ChannelUsers)
                .Include(c => c.ChannelCourses)
                .FirstOrDefaultAsync(c => c.ChannelId == id);

            if (channel == null)
                return false;

            channel.IsActive = false;
            channel.UpdatedAt = DateTime.UtcNow;

            foreach (var channelUser in channel.ChannelUsers)
            {
                channelUser.isActive = false;
                channelUser.LastUpdatedAt = DateTime.UtcNow;
            }

            foreach (var channelCourse in channel.ChannelCourses)
            {
                channelCourse.IsActive = false;
                channelCourse.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsChannelExists(Guid id)
        {
            var channel = await _context.Channels.FindAsync(id);
            return channel != null;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Channel?>?> GetChannelsByUserIdAsync(Guid userId, string userType)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == userType);
            if (role == null) return null;

            var channelIds = await (from cu in _context.ChannelUsers
                                    join cur in _context.ChannelUserRoles on cu.ChannelUserId equals cur.ChannelUserId
                                    where cu.UserId == userId && cur.RoleId == role.Id
                                    select cu.ChannelId).ToListAsync();

            return await _context.Channels.Where(c => channelIds.Contains(c.ChannelId)).ToListAsync();
        }

        public async Task<Channel?> GetChannelByUserIdAsync(Guid channelId, Guid userId, string userType)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == userType);
            if (role == null) return null;

            var channel = await (from cu in _context.ChannelUsers
                                join cur in _context.ChannelUserRoles on cu.ChannelUserId equals cur.ChannelUserId
                                join c in _context.Channels on cu.ChannelId equals c.ChannelId
                                where c.ChannelId == channelId && cu.UserId == userId && cur.RoleId == role.Id
                                select c).FirstOrDefaultAsync();

            return channel;
        }
    }
}
