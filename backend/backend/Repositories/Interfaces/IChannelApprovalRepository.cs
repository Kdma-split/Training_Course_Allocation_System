namespace backend.Repositories.Interfaces
{
    using backend.Models;

    public interface IChannelApprovalsRepository
    {
        Task<bool> IsApprovalExists(Guid id);
        Task<ChannelApproval?> CreateApproval(Guid channelId, Guid userId, ChannelApproval approval);
        Task<bool> ChangeStatusApproval(Guid id, string status);
        Task<bool> UpdateChannelApprovalStatus(Guid channelApprovalId, string status);
        Task<ChannelApproval> GetApprovalsByChannelIdUserId(Guid channelId, Guid userId);
        Task<ChannelApproval?> GetChannelApprovalById(Guid channelApprovalId);
        Task<IEnumerable<ChannelApproval>> GetPendingApprovalsByChannelId(Guid channelId);
        Task<bool> SoftDeleteApprovalAsync(Guid channelId, Guid userId);
    }
}
