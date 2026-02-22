namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class ChannelRepository : IChannelRepository
    {
        private readonly TrainingCourseContext _context;

        public ChannelRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<Channel?> GetChannelByIdAsync(Guid id)
        {
            return await _context.Channels.FindAsync(id);
        }

        public async Task<Channel?> GetChannelByUserId(Guid channelId, Guid userId, string userType)
        {
            //return await _context.Channels.FindAsync(channelId, userId, userType);
            if (userType == "admin")
            {
                return await _context.Channels.Include().Where(
                    c => c.ChannelId == channelId,
                    c => c.AdminId == userId
                );
            }
            return await _context.Channels.Include().Where(
                c => c.ChannelId == channelId,
                c => c.CreatedById == userId
            );
        }


        public async Task<Channel?> GetChannelsByUserId(Guid userId, string userType)
        {
            //return await _context.Channels.FindAsync(channelId, userId, userType);
            if (userType == "admin")
            {
                return await _context.Channels.Include().Where(
                    c => c.AdminId == userId
                );
            }
            return await _context.Channels.Include().Where(
                c => c.CreatedById == userId
            );
        }


        public async Task<IEnumerable<Channel>> GetAllChannelsAsync()
        {
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

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
