namespace backend.Controllers
{
    using backend.Data;
    using backend.Dto.ChannelRoles;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChannelUserController : ControllerBase
    {
        private readonly IChannelUserRepository _channelUserRepository;
        private readonly IChannelUserRoleRepository _channelUserRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly TrainingCourseContext _context;

        public ChannelUserController(
            IChannelUserRepository channelUserRepository,
            IChannelUserRoleRepository channelUserRoleRepository,
            IUserRepository userRepository,
            TrainingCourseContext context
        )
        {
            _channelUserRepository = channelUserRepository;
            _channelUserRoleRepository = channelUserRoleRepository;
            _userRepository = userRepository;
            _context = context;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        private async Task<bool> IsChannelAdminAsync(Guid channelId, Guid userId)
        {
            var roles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            return roles != null && roles.Contains("ChannelAdmin");
        }

        private async Task<bool> IsChannelMemberAsync(Guid channelId, Guid userId)
        {
            var roles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            return roles != null;
        }

        private async Task<bool> IsMemberOrAdminAsync(Guid channelId, Guid userId)
        {
            return await _channelUserRoleRepository.GetMemberOrAdminAsync(channelId, userId);
        }

        private async Task<bool> HasOtherChannelAdminsAsync(Guid channelId, Guid excludeUserId)
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "ChannelAdmin");
            if (adminRole == null) return false;

            var otherAdmins = await (from cu in _context.ChannelUsers
                                    join cur in _context.ChannelUserRoles on cu.ChannelUserId equals cur.ChannelUserId
                                    where cu.ChannelId == channelId && cu.UserId != excludeUserId && cur.RoleId == adminRole.Id
                                    select cu.UserId).ToListAsync();

            return otherAdmins.Any();
        }

        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetChannelMembers(Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsMemberOrAdminAsync(channelId, userId))
                return Forbid();

            var channelUsers = await _channelUserRepository.GetUsersByChannelAsync(channelId);
            var members = new List<object>();

            foreach (var cu in channelUsers)
            {
                var user = await _userRepository.GetUserByIdAsync(cu.UserId);
                if (user != null)
                {
                    var userRoles = await _channelUserRoleRepository.GetUserRolesByChannelUserIdAsync(cu.ChannelUserId);

                    members.Add(new
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Roles = userRoles
                    });
                }
            }

            return Ok(members);
        }

        [HttpPost("channel/{channelId}")]
        public async Task<IActionResult> AddMember(Guid channelId, [FromBody] AddMemberDto dto)
        {
            var userId = GetUserId();

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid();

            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser == null)
                return NotFound(new { message = "User not found" });

            var existingChannelUser = await _channelUserRepository.GetChannelUserAsync(channelId, existingUser.UserId);
            if (existingChannelUser != null)
                return BadRequest(new { message = "User is already a member of this channel" });

            var channelUser = new ChannelUser
            {
                ChannelId = channelId,
                UserId = existingUser.UserId
            };

            await _channelUserRepository.AddUserToChannelAsync(channelUser);

            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                await _channelUserRoleRepository.AddRolesBulkByChannelUserIdAsync(channelUser.ChannelUserId, dto.RoleIds);
            }

            return Ok(new { message = "Member added successfully" });
        }

        [HttpDelete("channel/{channelId}/user/{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid channelId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsMemberOrAdminAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var channelUserToDelete = await _channelUserRepository.GetChannelUserAsync(channelId, memberId);
            if (channelUserToDelete == null)
                return NotFound(new { message = "Member not found in channel" });

            var currentUserIsAdmin = await IsChannelAdminAsync(channelId, userId);
            var userToDeleteIsAdmin = await IsChannelAdminAsync(channelId, memberId);

            if (currentUserIsAdmin)
            {
                if (userId == memberId)
                {
                    if (userToDeleteIsAdmin && !await HasOtherChannelAdminsAsync(channelId, memberId))
                        return BadRequest(new { message = "Cannot remove yourself as the only channel admin. Assign another admin first." });

                    await SoftDeleteChannelUserAsync(channelId, memberId);
                    return Ok(new { message = "You have removed yourself from the channel" });
                }

                await SoftDeleteChannelUserAsync(channelId, memberId);
                return Ok(new { message = "Member removed successfully" });
            }

            if (userId != memberId)
                return Forbid("Members can only remove themselves from the channel");

            await SoftDeleteChannelUserAsync(channelId, memberId);
            return Ok(new { message = "You have left the channel" });
        }

        [HttpDelete("channel/{channelId}/users/bulk")]
        public async Task<IActionResult> RemoveMembersBulk(Guid channelId, [FromBody] List<Guid> memberIds)
        {
            var userId = GetUserId();

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only channel admins can bulk remove members");

            if (memberIds == null || !memberIds.Any())
                return BadRequest(new { message = "No member IDs provided" });

            var deletedMembers = new List<Guid>();
            var failedMembers = new List<Guid>();

            foreach (var memberId in memberIds)
            {
                var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, memberId);
                if (channelUser != null)
                {
                    var isAdmin = await IsChannelAdminAsync(channelId, memberId);
                    if (isAdmin && memberId == userId && !await HasOtherChannelAdminsAsync(channelId, memberId))
                    {
                        failedMembers.Add(memberId);
                        continue;
                    }

                    await SoftDeleteChannelUserAsync(channelId, memberId);
                    deletedMembers.Add(memberId);
                }
                else
                {
                    failedMembers.Add(memberId);
                }
            }

            return Ok(new 
            { 
                message = $"Deleted {deletedMembers.Count} members, {failedMembers.Count} failed",
                deletedMembers = deletedMembers,
                failedMembers = failedMembers
            });
        }

        [HttpPut("channel/{channelId}/user/{memberId}")]
        public async Task<IActionResult> UpdateMemberRole(Guid channelId, Guid memberId, [FromBody] UpdateMemberRoleDto dto)
        {
            var userId = GetUserId();

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid();

            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, memberId);
            if (channelUser == null)
                return NotFound(new { message = "Member not found in channel" });

            var userRoles = await _channelUserRoleRepository.GetUserRolesByChannelUserIdAsync(channelUser.ChannelUserId);

            if (userRoles.Contains("ChannelAdmin"))
                return BadRequest(new { message = "Cannot change the channel admin's role" });

            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                await _channelUserRoleRepository.UpdateRolesByChannelUserIdAsync(channelUser.ChannelUserId, dto.RoleIds);
            }

            return Ok(new { message = "Member role updated successfully" });
        }

        private async Task SoftDeleteChannelUserAsync(Guid channelId, Guid userId)
        {
            var channelUser = await _context.ChannelUsers
                .FirstOrDefaultAsync(cu => cu.ChannelId == channelId && cu.UserId == userId);

            if (channelUser != null)
            {
                channelUser.isActive = false;
                channelUser.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
