namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface IAssignmentRepository
    {
        Task<Assignment?> GetAssignmentByIdAsync(Guid id);
        Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
        Task<Assignment> CreateAssignmentAsync(Assignment assignment);
        Task<Assignment> UpdateAssignmentAsync(Assignment assignment);
        Task<bool> DeleteAssignmentAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
