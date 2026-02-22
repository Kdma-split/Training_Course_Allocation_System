using Microsoft.AspNetCore.Mvc;
using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IAssignmentRepo
    {
        public Task<IActionResult> GetAllAssignmentsAsync();
        public Task<IActionResult> GetAssignmentByIdAsync(int id);
        public Task<IActionResult> CreateAssignmentAsync(Assignment assignment);
        public Task<IActionResult> UpdateAssignmentAsync(Assignment assignment);
        public Task<IActionResult> DeleteAssignmentAsync(int id);
        public Task<IActionResult> SaveChangesAsync();
    }
}