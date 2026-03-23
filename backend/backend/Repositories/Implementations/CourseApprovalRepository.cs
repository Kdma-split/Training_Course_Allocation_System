namespace backend.Repositories.Implementations
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class CourseApprovalRepository : ICourseApprovalRepository
    {
        private readonly TrainingCourseContext _context;

        public CourseApprovalRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<CourseApproval?> GetCourseApprovalByIdAsync(Guid approvalId)
        {
            return await _context.CourseApprovals.FindAsync(approvalId);
        }

        // get all approvals for a specific course in a specific channel, regardless of status of the approval, can be used for studying the human psychlogy with the help of an ML model / LLM...
        public async Task<IEnumerable<CourseApproval>> GetApprovalsByChannelCourseIdAsync(int channelCourseId)
        {
            return await _context.CourseApprovals
                .Where(ca => ca.CourseChannelUserId == channelCourseId)
                .ToListAsync();
        }

        // create a new course approval request...
        public async Task<CourseApproval> CreateCourseApprovalAsync(CourseApproval approval)
        {
            _context.CourseApprovals.Add(approval);
            await _context.SaveChangesAsync();
            return approval;
        }

        // update the status of a course approval request...
        public async Task<bool> UpdateCourseApprovalStatusAsync(Guid approvalId, string status)
        {
            var approval = await _context.CourseApprovals.FindAsync(approvalId);
            if (approval == null) return false;

            approval.Status = status;
            approval.UpdatedAt = DateTime.UtcNow;
            _context.CourseApprovals.Update(approval);
            return await _context.SaveChangesAsync() > 0;
        }

        // gives all the approvals for a specific user, across all courses and channels, regardless of the status of the approval, can be used for studying the human psychlogy with the help of an ML model / LLM...
        public async Task<IEnumerable<CourseApproval>> GetAllApprovalsByUserIdAsync(Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.UserId == userId && ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }
        // delete a course approval request...
        public async Task<bool> DeleteCourseApprovalAsync(Guid approvalId)
        {
            var approval = await _context.CourseApprovals.FindAsync(approvalId);
            if (approval == null) return false;

            _context.CourseApprovals.Remove(approval);
            return await _context.SaveChangesAsync() > 0;
        }

        // gives all the approvals of all the users in all the courses under a specific channel, regardless of the status of the approval...
        public async Task<IEnumerable<CourseApproval>> GetAllApprovalsByChannelAsync(Guid channelId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null && 
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the approvals of all the users under a specific course and channel, regardless of the status of the approval...
        public async Task<IEnumerable<CourseApproval>> GetAllApprovalsByChannelCourseAsync(Guid channelId, Guid courseId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the approvals of all the users under a specific course and channel, regardless of the status of the approval...
        public async Task<IEnumerable<CourseApproval>> GetAllApprovalsByChannelCourseUserAsync(Guid channelId, Guid courseId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the pending approvals for all course and users, under a specific channel... 
        public async Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelAsync(Guid channelId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null && 
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.Status == "pending" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the pending approvals for all users, in a specific course under a specific channel... 
        public async Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelCourseAsync(Guid channelId, Guid courseId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.Status == "pending" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the pending approvals for all courses, under a specific user in a channel... 
        public async Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelCourseUserAsync(Guid channelId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "pending" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the pending approvals for a specific user in a particular course of a specific channel...
        public async Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelCourseAsync(Guid channelId, Guid courseId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "pending" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // get all approvals for a specific user, under a specific channel and course...
        public async Task<IEnumerable<CourseApproval>> GetApprovalsByChannelIdCourseIdUserIdAsync(Guid channelId, Guid courseId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // get all approvals for a specific user, under a specific channel across all courses...
        public async Task<IEnumerable<CourseApproval>> GetApprovalsByChannelIdUserIdAsync(Guid channelId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // get approved approvals for a specific user, under a specific channel across all courses...
        public async Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelIdUserIdAsync(Guid channelId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "approved" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // get rejected approvals for a specific user, under a specific channel across all courses...
        public async Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelIdUserIdAsync(Guid channelId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "rejected" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the approved approvals for all the users involved in all the courses, under a particular channel
        public async Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelAsync(Guid channelId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null && 
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.Status == "approved" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        // gives all the approved approvals for a specific user in a particular course of a specific channel...
        public async Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelCourseAsync(Guid channelId, Guid courseId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "approved" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelUserAsync(Guid channelId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "approved" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }
        // gives all the approved approvals for all users, in a specific course under a specific channel... 
        public async Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelCourseAsync(Guid channelId, Guid courseId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.Status == "approved" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }


        // gives all the rejected approvals for all the users involved in all the courses, under a particular channel
        public async Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelAsync(Guid channelId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null && 
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.Status == "rejected" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelCourseUserAsync(Guid channelId, Guid courseId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "rejected" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelUserAsync(Guid channelId, Guid userId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.UserId == userId &&
                            ca.Status == "rejected" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelCourseAsync(Guid channelId, Guid courseId)
        {
            return await _context.CourseApprovals
                .Include(ca => ca.CourseChannelUser)
                    .ThenInclude(ccu => ccu.ChannelCourse)
                .Where(ca => ca.CourseChannelUser.ChannelCourse != null &&
                            ca.CourseChannelUser.ChannelCourse.ChannelId == channelId &&
                            ca.CourseChannelUser.ChannelCourse.CourseId == courseId &&
                            ca.Status == "rejected" &&
                            ca.IsActive)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }
    }
}
