namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class CourseChannelUserRepository : ICourseChannelUserRepository
    {
        private readonly TrainingCourseContext _context;

        public CourseChannelUserRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<CourseChannelUser?> GetByUserIdAndChannelIdAsync(Guid userId, Guid channelId)
        {
            return await _context.CourseChannelUsers
                .Include(ccu => ccu.ChannelCourse)
                .FirstOrDefaultAsync(ccu => ccu.UserId == userId && ccu.ChannelCourse != null && ccu.ChannelCourse.ChannelId == channelId);
        }

        public async Task<CourseChannelUser> CreateAsync(CourseChannelUser courseChannelUser)
        {
            _context.CourseChannelUsers.Add(courseChannelUser);
            await _context.SaveChangesAsync();
            return courseChannelUser;
        }
    }
}