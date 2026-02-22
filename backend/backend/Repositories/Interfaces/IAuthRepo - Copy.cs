using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IAssignmentRepo
    {
        Task<IEnumerable<Assignment>> GetAllAssignmentsByDomainAsync();
        //Task<IEnumerable<Assignment>?> GetAssignmentByUserAsync();
        Task<Assignment> GetAssignmentByIdAsync(int id);
        Task CreateAssignmentAsync(Assignment assignment);
        Task UpdateAssignmentAsync(Assignment assignment);
        Task DeleteAssignmentAsync(int id);
    }

}