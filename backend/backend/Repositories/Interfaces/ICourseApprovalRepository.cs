using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface ICourseApprovalRepository
    {
        Task<CourseApproval?> GetCourseApprovalByIdAsync(Guid approvalId);
        Task<IEnumerable<CourseApproval>> GetApprovalsByChannelCourseIdAsync(int channelCourseId);
        Task<CourseApproval> CreateCourseApprovalAsync(CourseApproval approval);
        Task<bool> UpdateCourseApprovalStatusAsync(Guid approvalId, string status);
        Task<IEnumerable<CourseApproval>> GetAllApprovalsByUserIdAsync(Guid userId);
        Task<bool> DeleteCourseApprovalAsync(Guid approvalId);
        Task<IEnumerable<CourseApproval>> GetAllApprovalsByChannelAsync(Guid channelId);
        Task<IEnumerable<CourseApproval>> GetAllApprovalsByChannelCourseAsync(Guid channelId, Guid courseId);
        Task<IEnumerable<CourseApproval>> GetAllApprovalsByChannelCourseUserAsync(Guid channelId, Guid courseId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelAsync(Guid channelId);
        Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelCourseAsync(Guid channelId, Guid courseId);
        Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelCourseUserAsync(Guid channelId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetPendingApprovalsByChannelCourseAsync(Guid channelId, Guid courseId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelAsync(Guid channelId);
        Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelCourseAsync(Guid channelId, Guid courseId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelUserAsync(Guid channelId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelCourseAsync(Guid channelId, Guid courseId);
        Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelAsync(Guid channelId);
        Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelCourseUserAsync(Guid channelId, Guid courseId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelUserAsync(Guid channelId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelCourseAsync(Guid channelId, Guid courseId);
        Task<IEnumerable<CourseApproval>> GetApprovalsByChannelIdCourseIdUserIdAsync(Guid channelId, Guid courseId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetApprovalsByChannelIdUserIdAsync(Guid channelId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetApprovedApprovalsByChannelIdUserIdAsync(Guid channelId, Guid userId);
        Task<IEnumerable<CourseApproval>> GetRejectedApprovalsByChannelIdUserIdAsync(Guid channelId, Guid userId);
    }
}
