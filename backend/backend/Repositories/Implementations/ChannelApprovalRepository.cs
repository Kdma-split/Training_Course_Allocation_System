using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Implementations
{
    public class ChannelApprovalRepository : IChannelApprovalsRepository
    {
        TrainingCourseContext _context;
        public ChannelApprovalRepository(TrainingCourseContext context)
        {
            _context = context;
        }

        public async Task<bool> IsApprovalExists(Guid id)
        {
            //return await _context.ChannelApprovals.AnyAsync(ca => ca.ChannelApprovalId == id);
            //return await _context.ChannelApprovals.AnyAsync(ca => ca.ChannelApprovalId == id && ca.IsActive == true);
            return await _context.ChannelApprovals.AnyAsync(ca => ca.ChannelApprovalId == id && ca.IsActive);
        }

        public async Task<ChannelApproval?> CreateApproval(Guid channelId, Guid userId, ChannelApproval approval)
        {
            var channelUser = await _context.ChannelUsers
                .FirstOrDefaultAsync(cu => cu.ChannelId == channelId && cu.UserId == userId);

            if (channelUser != null)
            {
            // DON'T ADD A USER...

                //channelUser = new ChannelUser
                //{
                //    ChannelId = channelId,
                //    UserId = userId,
                //    Role = Role.Viewer,
                //    isActive = true
                ////};
                //_context.ChannelUsers.Add(channelUser);
                //await _context.SaveChangesAsync();

                approval.ChannelUserId = channelUser.ChannelUserId;
                approval.Status = "pending";
                approval.IsActive = true;
                approval.CreatedAt = DateTime.UtcNow;
                approval.UpdatedAt = DateTime.UtcNow;
            }

            _context.ChannelApprovals.Add(approval);
            await _context.SaveChangesAsync();

            return approval;
        }

        public async Task<bool> UpdateChannelApprovalStatus(Guid channelApprovalId, string status)
        {
            var approval = await _context.ChannelApprovals.FindAsync(channelApprovalId);
            if (approval == null)
                return false;

            if (status == "reject")
            {
                approval.Status = "rejected";
            }
            else if (status == "approve")
            {
                approval.Status = "approved";
            }

            approval.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeStatusApproval(Guid id, string status)
        {
            var approval = await _context.ChannelApprovals.FindAsync(id);
            if (approval == null)
                return false;

            approval.Status = status;
            approval.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ChannelApproval> GetApprovalsByChannelIdUserId(Guid channelId, Guid userId)
        {
            var channelUser = await _context.ChannelUsers
                .FirstOrDefaultAsync(cu => cu.ChannelId == channelId && cu.UserId == userId);

            if (channelUser == null)
                return null!;

            var approval = await _context.ChannelApprovals
                .FirstOrDefaultAsync(ca => ca.ChannelUserId == channelUser.ChannelUserId && ca.IsActive == true);

            return approval!;
        }

        public async Task<ChannelApproval?> GetChannelApprovalById(Guid channelApprovalId)
        {
            return await _context.ChannelApprovals.FindAsync(channelApprovalId);
        }

        public async Task<IEnumerable<ChannelApproval>> GetPendingApprovalsByChannelId(Guid channelId)
        {
            var channelUsers = await _context.ChannelUsers
                .Where(cu => cu.ChannelId == channelId)
                .Select(cu => cu.ChannelUserId)
                .ToListAsync();

            return await _context.ChannelApprovals
                .Where(ca => channelUsers.Contains(ca.ChannelUserId) && ca.Status == "pending" && ca.IsActive)
                .ToListAsync();
        }

        public async Task<bool> SoftDeleteApprovalAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _context.ChannelUsers
                .FirstOrDefaultAsync(cu => cu.ChannelId == channelId && cu.UserId == userId);

            if (channelUser == null)
                return false;

            var approval = await _context.ChannelApprovals
                .FirstOrDefaultAsync(ca => ca.ChannelUserId == channelUser.ChannelUserId);

            approval.IsActive = false;
            approval.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
