namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class ChannelAssignmentRepository : IChannelAssignmentRepository
    {
        private readonly TrainingCourseContext _context;

        public ChannelAssignmentRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<ChannelAssignment?> GetChannelAssignmentAsync(Guid channelId, Guid assignmentId)
        {
            return await _context.ChannelAssignments
                .FirstOrDefaultAsync(ca => ca.ChannelId == channelId && ca.AssignmentId == assignmentId);
        }

        public async Task<IEnumerable<ChannelAssignment>> GetAssignmentsByChannelAsync(Guid channelId)
        {
            return await _context.ChannelAssignments
                .Where(ca => ca.ChannelId == channelId)
                .Include(ca => ca.Assignment)
                .ToListAsync();
        }

        public async Task<ChannelAssignment> AddAssignmentToChannelAsync(ChannelAssignment channelAssignment)
        {
            _context.ChannelAssignments.Add(channelAssignment);
            await _context.SaveChangesAsync();
            return channelAssignment;
        }

        public async Task<ChannelAssignment> UpdateChannelAssignmentAsync(ChannelAssignment channelAssignment)
        {
            _context.ChannelAssignments.Update(channelAssignment);
            await _context.SaveChangesAsync();
            return channelAssignment;
        }

        public async Task<bool> RemoveAssignmentFromChannelAsync(Guid channelId, Guid assignmentId)
        {
            var channelAssignment = await _context.ChannelAssignments
                .FirstOrDefaultAsync(ca => ca.ChannelId == channelId && ca.AssignmentId == assignmentId);
            
            if (channelAssignment == null)
                return false;

            _context.ChannelAssignments.Remove(channelAssignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
