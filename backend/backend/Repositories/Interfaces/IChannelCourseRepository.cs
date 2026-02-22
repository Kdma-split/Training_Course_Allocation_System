namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface IChannelCourseRepository
    {
        Task<ChannelCourse?> GetChannelCourseAsync(Guid channelId, Guid courseId);
        Task<IEnumerable<ChannelCourse>> GetCoursesByChannelAsync(Guid channelId);
        Task<ChannelCourse> AddCourseToChannelAsync(ChannelCourse channelCourse);
        Task<ChannelCourse> UpdateChannelCourseAsync(ChannelCourse channelCourse);
        Task<bool> RemoveCourseFromChannelAsync(Guid channelId, Guid courseId);
        Task<bool> SaveChangesAsync();
    }
}
