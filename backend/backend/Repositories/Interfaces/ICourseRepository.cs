using backend.Models;

namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface ICourseRepository
    {
        Task<Course?> GetCourseByIdAsync(Guid id);
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course> CreateCourseAsync(Course course);
        Task<Course> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(Guid id);
        Task<bool> IsCourseExistsByDomainAsync(Guid courseId, Guid domainId);
        Task<bool> SaveChangesAsync();
    }
}

