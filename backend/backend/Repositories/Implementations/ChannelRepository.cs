using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Implementations
{
    public class ChannelRepository
    {
        private TrainingCourseContext _context;
        ChannelRepository(TrainingCourseContext context) { 
            _context = context;
        }

        public async Task<Channel?> GetChannelAsync(Guid id)
        {
            return _context.Channels.SingleOrDefaultAsync(c => c.ChannelId == id);
        }
    }
}