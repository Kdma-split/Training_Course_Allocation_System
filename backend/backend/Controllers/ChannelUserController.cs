namespace backend.Controllers
{
    using backend.Models;
    using backend.Repositories.Interfaces;
    using backend.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChannelUserController : ControllerBase
    {
        private readonly IChannelUserRepository _channelUserRepository;
        private readonly IChannelUserPermissionService _channelUserPermissionService;
        private readonly IUserRepository _userRepository;

        public ChannelUserController(
            IChannelUserRepository channelUserRepository,
            IChannelUserPermissionService channelUserPermissionService,
            IUserRepository userRepository)
        {
            _channelUserRepository = channelUserRepository;
            _channelUserPermissionService = channelUserPermissionService;
            _userRepository = userRepository;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetChannelMembers(Guid channelId)
        {
            var userId = GetUserId();

            var role = await _channelUserPermissionService.GetUserRoleInChannelAsync(channelId, userId);
            if (role == null)
                return Forbid();

            var channelUsers = await _channelUserRepository.GetUsersByChannelAsync(channelId);
            var members = new List<object>();

            foreach (var cu in channelUsers)
            {
                var user = await _userRepository.GetUserByIdAsync(cu.UserId);
                if (user != null)
                {
                    members.Add(new
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = cu.Role
                    });
                }
            }

            return Ok(members);
        }

        [HttpPost("channel/{channelId}")]
        public async Task<IActionResult> AddMember(Guid channelId, [FromBody] AddMemberDto dto)
        {
            var userId = GetUserId();

            if (!await _channelUserPermissionService.CanAddMembersAsync(channelId, userId))
                return Forbid();

            var existingUser = await _userRepository.GetUserByIdAsync(dto.UserId);
            if (existingUser == null)
                return NotFound(new { message = "User not found" });

            var existingChannelUser = await _channelUserRepository.GetChannelUserAsync(channelId, dto.UserId);
            if (existingChannelUser != null)
                return BadRequest(new { message = "User is already a member of this channel" });

            var channelUser = new ChannelUser
            {
                ChannelId = channelId,
                UserId = dto.UserId,
                Role = dto.Role ?? Role.Viewer
            };

            await _channelUserRepository.AddUserToChannelAsync(channelUser);

            return Ok(new { message = "Member added successfully" });
        }

        [HttpDelete("channel/{channelId}/user/{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid channelId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await _channelUserPermissionService.CanRemoveMembersAsync(channelId, userId))
                return Forbid();

            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, memberId);
            if (channelUser == null)
                return NotFound(new { message = "Member not found in channel" });

            if (channelUser.Role == Role.Author)
                return BadRequest(new { message = "Cannot remove the channel author" });

            await _channelUserRepository.RemoveUserFromChannelAsync(channelId, memberId);

            return Ok(new { message = "Member removed successfully" });
        }

        [HttpPut("channel/{channelId}/user/{memberId}")]
        public async Task<IActionResult> UpdateMemberRole(Guid channelId, Guid memberId, [FromBody] UpdateMemberRoleDto dto)
        {
            var userId = GetUserId();

            if (!await _channelUserPermissionService.CanAddMembersAsync(channelId, userId))
                return Forbid();

            var channelUser = await _channelUserRepository.GetChannelUserAsync(channelId, memberId);
            if (channelUser == null)
                return NotFound(new { message = "Member not found in channel" });

            if (channelUser.Role == Role.Author)
                return BadRequest(new { message = "Cannot change the channel author's role" });

            channelUser.Role = dto.Role;
            await _channelUserRepository.UpdateUserRoleAsync(channelUser);

            return Ok(new { message = "Member role updated successfully" });
        }
    }

    public class AddMemberDto
    {
        public Guid UserId { get; set; }
        public Role? Role { get; set; }
    }

    public class UpdateMemberRoleDto
    {
        public Role Role { get; set; }
    }
}
