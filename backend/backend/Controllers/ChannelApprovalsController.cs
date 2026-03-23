using Azure;
using backend.Data;
using backend.Dto.Channel;
using backend.Models;
using backend.Repositories.Implementations;
using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/{[Controller]}")]
    public class ChannelsApprovalController : ControllerBase
    {
        IUserRepository _users;
        IChannelApprovalsRepository _channelApprovals;
        IChannelUserRepository _channelUsers;
        IChannelUserRoleRepository _channelUserRoleRepository;
        private readonly TrainingCourseContext _context;

        public ChannelsApprovalController(
            IChannelApprovalsRepository channelApprovals,
            IUserRepository users,
            IChannelUserRepository channelUsers,
            IChannelUserRoleRepository channelUserRoleRepository,
            TrainingCourseContext context
        )
        {
            _channelApprovals = channelApprovals;
            _users = users;
            _channelUsers = channelUsers;
            _channelUserRoleRepository = channelUserRoleRepository;
            _context = context;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("{channelId}/{userId}")]
        public async Task<IActionResult> ListApprovalsById(Guid channelId, Guid userId)
        {
            var currentUserId = GetUserId();

            var userRoles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, currentUserId);
            if (userRoles == null || !userRoles.Contains("ChannelAdmin"))
                return Forbid();

            var approvals = await _channelApprovals.GetApprovalsByChannelIdUserId(channelId, userId);

            if (approvals == null)
                return NotFound(new { message = "No approvals found" });

            var approvalDesc = !string.IsNullOrEmpty(approvals.ApprovalDescription)
                ? System.Text.Json.JsonSerializer.Deserialize<ChannelApprovalUserDto>(approvals.ApprovalDescription)
                : null;

            var response = new ResponseChannelApprovalDto
            {
                ChannelApprovalId = approvals.ChannelApprovalId,
                ChannelUserId = approvals.ChannelUserId,
                ApprovalDescription = approvalDesc,
                Status = approvals.Status,
                isActived = approvals.IsActive,
                CreatedAt = approvals.CreatedAt,
                UpdatedAt = approvals.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost("{channelId}")]
        public async Task<IActionResult> SendApproval(Guid channelId, [FromBody] CreateChannelApprovalDto dto)
        {
            var userId = GetUserId();

            var userRoles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            if (userRoles != null && userRoles.Contains("ChannelAdmin"))
                return Forbid();

            ChannelApprovalUserDto approvalDetails = new()
            {
                Name = dto.ApprovalUserName,
                Email = dto.ApprovalUserEmail,
                Age = dto.ApprovalUserAge,
                Role = dto.ApprovalUserRole
            };

            var user = await _users.GetUserByEmailAsync(approvalDetails.Email);
            if (user != null)
                return BadRequest(new { message = "User already exists in the system" });

            var approval = new ChannelApproval
            {
                ApprovalDescription = System.Text.Json.JsonSerializer.Serialize(approvalDetails)
            };

            var response = await _channelApprovals.CreateApproval(channelId, userId, approval);

            var responseDesc = !string.IsNullOrEmpty(response.ApprovalDescription)
                ? System.Text.Json.JsonSerializer.Deserialize<ChannelApprovalUserDto>(response.ApprovalDescription)
                : null;

            var responseDto = new ResponseChannelApprovalDto
            {
                ChannelApprovalId = response.ChannelApprovalId,
                ChannelUserId = response.ChannelUserId,
                ApprovalDescription = responseDesc,
                Status = response.Status,
                isActived = response.IsActive,
                CreatedAt = response.CreatedAt,
                UpdatedAt = response.UpdatedAt
            };

            return CreatedAtAction(nameof(ListApprovalsById), new { channelId, userId }, responseDto);
        }

        [HttpGet("/Accept/{channelId}/{channelApprovalId}")]
        public async Task<IActionResult> AcceptApprovals(Guid channelId, Guid channelApprovalId)
        {
            var userId = GetUserId();

            var userRoles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            if (userRoles == null || !userRoles.Contains("ChannelAdmin"))
                return Forbid();

            var channelApprovalDetails = await _channelApprovals.GetChannelApprovalById(channelApprovalId);

            if (channelApprovalDetails == null)
                return NotFound(new { message = "Approval not found" });

            var approvalDesc = System.Text.Json.JsonSerializer.Deserialize<ChannelApprovalUserDto>(channelApprovalDetails.ApprovalDescription);
            if (approvalDesc == null)
                return BadRequest(new { message = "Invalid approval details" });

            var user = await _users.GetUserByEmailAsync(approvalDesc.Email);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var existingChannelUser = await _channelUsers.GetChannelUserAsync(channelId, user.UserId);
            if (existingChannelUser != null)
                return BadRequest(new { message = "User is already a member of this channel" });

            var channelUserCreate = new ChannelUser
            {
                ChannelId = channelId,
                UserId = user.UserId
            };

            await _channelUsers.AddUserToChannelAsync(channelUserCreate);

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == approvalDesc.Role);
            if (role != null)
            {
                var channelUserRole = new ChannelUserRoles
                {
                    Id = Guid.NewGuid(),
                    ChannelUserId = channelUserCreate.ChannelUserId,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.ChannelUserRoles.AddAsync(channelUserRole);
                await _context.SaveChangesAsync();
            }

            await _channelApprovals.UpdateChannelApprovalStatus(channelApprovalId, "approve");

            return Ok(new { message = "Member added successfully" });
        }

        [HttpGet("/Reject/{channelId}/{channelApprovalId}")]
        public async Task<IActionResult> RejectApprovals(Guid channelId, Guid channelApprovalId)
        {
            var userId = GetUserId();

            var userRoles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            if (userRoles == null || !userRoles.Contains("ChannelAdmin"))
                return Forbid();

            var channelApprovalDetails = await _channelApprovals.GetChannelApprovalById(channelApprovalId);

            if (channelApprovalDetails == null)
                return NotFound(new { message = "Approval not found" });

            await _channelApprovals.UpdateChannelApprovalStatus(channelApprovalId, "reject");

            return Ok(new { message = "Approval rejected successfully" });
        }
    }
}
