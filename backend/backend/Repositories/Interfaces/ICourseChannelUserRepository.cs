namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface ICourseChannelUserRepository
    {
        Task<CourseChannelUser?> GetByUserIdAndChannelIdAsync(Guid userId, Guid channelId);
        Task<CourseChannelUser> CreateAsync(CourseChannelUser courseChannelUser);
    }
}