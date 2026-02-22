namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly TrainingCourseContext _context;

        public AssignmentRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(Guid id)
        {
            return await _context.Assignments.FindAsync(id);
        }

        public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
        {
            return await _context.Assignments.ToListAsync();
        }

        public async Task<Assignment> CreateAssignmentAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<Assignment> UpdateAssignmentAsync(Assignment assignment)
        {
            _context.Assignments.Update(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> DeleteAssignmentAsync(Guid id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
                return false;

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
