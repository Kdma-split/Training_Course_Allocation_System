namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class ChannelCourseRepository : IChannelCourseRepository
    {
        private readonly TrainingCourseContext _context;

        public ChannelCourseRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<ChannelCourse?> GetChannelCourseAsync(Guid channelId, Guid courseId)
        {
            return await _context.ChannelCourses
                .FirstOrDefaultAsync(cc => cc.ChannelId == channelId && cc.CourseId == courseId);
        }

        public async Task<ChannelCourse?> GetChannelCourseByIdAsync(Guid channelCourseId)
        {
            return await _context.ChannelCourses
                .FirstOrDefaultAsync(cc => cc.ChannelCourseId == channelCourseId);
        }

        public async Task<ChannelCourse?> GetFirstChannelCourseByChannelIdAsync(Guid channelId)
        {
            return await _context.ChannelCourses
                .FirstOrDefaultAsync(cc => cc.ChannelId == channelId);
        }

        public async Task<IEnumerable<ChannelCourse>> GetCoursesByChannelAsync(Guid channelId)
        {
            return await _context.ChannelCourses
                .Where(cc => cc.ChannelId == channelId)
                .Include(cc => cc.Course)
                .ToListAsync();
        }

        public async Task<ChannelCourse> AddCourseToChannelAsync(ChannelCourse channelCourse)
        {
            _context.ChannelCourses.Add(channelCourse);
            await _context.SaveChangesAsync();
            return channelCourse;
        }

        public async Task<ChannelCourse> UpdateChannelCourseAsync(ChannelCourse channelCourse)
        {
            _context.ChannelCourses.Update(channelCourse);
            await _context.SaveChangesAsync();
            return channelCourse;
        }

        public async Task<bool> RemoveCourseFromChannelAsync(Guid channelId, Guid courseId)
        {
            var channelCourse = await _context.ChannelCourses
                .FirstOrDefaultAsync(cc => cc.ChannelId == channelId && cc.CourseId == courseId);
            
            if (channelCourse == null)
                return false;

            _context.ChannelCourses.Remove(channelCourse);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveCourseFromChannelAsync(Guid channelCourseId)
        {
            var channelCourse = await _context.ChannelCourses
                .FirstOrDefaultAsync(cc => cc.ChannelCourseId == channelCourseId);
            
            if (channelCourse == null)
                return false;

            _context.ChannelCourses.Remove(channelCourse);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
