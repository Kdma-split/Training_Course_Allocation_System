using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<string?> GetUserRole(Guid id);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByRoleAsync(string role);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(Guid id, User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> IsUserExistsAsync(string email);
        Task<bool> SaveChangesAsync();
        Task<string?> GetUserRoleAsync(Guid userId);
    }
}
