using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface ICourseRepo
    {
        Task<Course> CreateCourseAsync(Course course);
        Task<Course?> GetCourseByIdAsync(Guid courseId);
        Task<IEnumerable<Course?>?> GetAllCoursesAsync();
        Task<Course> UpdateCourseAsync(Guid courseId, Course updatedCourse);
        Task<bool> DeleteCourseAsync(Guid courseId);  // returns true if deletion was successful, false otherwise
        Task<bool> IsCourseExistsAsync(string domain);
        Task<bool> SaveChangesAsync();
    }
}