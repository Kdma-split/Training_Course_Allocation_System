using Azure;
using backend.Dto.Channel;
using backend.Models;
using backend.Repositories.Implementations;
using backend.Repositories.Interfaces;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

// NO NEED FOR CHECKING THE ECXISTENCE OF USERS IN OTHER TABLE, AS THE USER CANNOT ACCESS THE FRONTEND THIS FAR IF HE'S NOT A USER OF THIS APPLICATION, THUS, CAN BE SAID THAT THIS WAY OF ADDING / REMOVING A MEMBER FROM THE CHANNEL IS STRICTLY RESTRICTED TO THE WEB APPLICATION USERS...


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
        IChannelPermissionService _channelPermissionService;

        public ChannelsApprovalController(
            IChannelApprovalsRepository channelApprovals,
            IUserRepository users,
            IChannelUserRepository channelUsers,
            IChannelPermissionService permissionService
        )
        {
            _channelApprovals = channelApprovals;
            _users = users;
            _channelUsers = channelUsers;
            _channelPermissionService = permissionService;
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

            if (!await _channelPermissionService.CanEditApprovalAsync(channelId, currentUserId))
                return Forbid();

            var approvals = await _channelApprovals.GetApprovalsByChannelIdUserId(channelId, userId);

            if (approvals == null)
                return NotFound(new { message = "No approvals found" });

            var response = new ResponseChannelApprovalDto
            {
                ChannelApprovalId = approvals.ChannelApprovalId,
                ChannelUserId = approvals.ChannelUserId,
                ApprovalDescription = approvals.ApprovalDescription,
                Status = approvals.Status,
                isActived = approvals.isActived,
                CreatedAt = approvals.CreatedAt,
                UpdatedAt = approvals.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost("{channelId}")]
        public async Task<IActionResult> SendApproval(Guid channelId, [FromBody] CreateChannelApprovalDto dto)
        {
            var userId = GetUserId();

            // ADMIN CANNOT CREATE ANY APPROVAL, HE CAN DIRECTLY ADD MEMBERS...
            var userRole = await _channelPermissionService.GetUserRoleInChannelAsync(channelId, userId);
            if (userRole == Role.Admin)
                return Forbid();

            ChannelApprovalDetails approvalDetails = new()
            {
                Name = dto.ApprovalUserName,
                Email = dto.ApprovalUserEmail,
                Age = dto.ApprovalUserAge,
                Role = dto.ApprovalUserRole
            };

            // HERE, WE ARE ASSUMING THAT THE USER TO BE APPROVED TO ALLOW TO JOIN THE CHANNEL HAS ALREADY JOINED THE CHANNEL, OTHERWISE AUTOMATIC ADDING THE USER TO THE CHANNEL WOULD ADD ADDITIONAL OVERHEAD OF ASSIGNING AN EXTRA PASSWORD AND NAME TO THE USER BEING AOOROVED AND PROVIDING PROVISION TO ALLOW THE USERS TO CHANGE THEIR CREDENTIALS LATER... EXTRA DEVELOPMENT OVERHEAD...

            var user = await _users.GetUserByEmailAsync(approvalDetails.Email);
            if (user != null)
                return BadRequest(new { message = "User already exists in the system" });

            var approval = new ChannelApproval
            {
                ApprovalDescription = System.Text.Json.JsonSerializer.Serialize(approvalDetails)
            };

            var response = await _channelApprovals.CreateApproval(channelId, userId, approval);

            var responseDto = new ResponseChannelApprovalDto
            {
                ChannelApprovalId = response.ChannelApprovalId,
                ChannelUserId = response.ChannelUserId,
                ApprovalDescription = response.ApprovalDescription,
                Status = response.Status,
                isActived = response.isActived,
                CreatedAt = response.CreatedAt,
                UpdatedAt = response.UpdatedAt
            };

            return CreatedAtAction(nameof(ListApprovalsById), new { channelId, userId }, responseDto);
        }

        //[HttpPost("/ResolveApproval/{id}")]
        //public async IActionResult<bool> ResolveApprovals(Guid id, [FromBody] ChannelApprovalDto dto)
        [HttpGet("/Resolve/{channelId}/{channelApprovalId}")]
        public async Task<IActionResult> ResolveApprovals(Guid channelId, Guid channelApprovalId)
        {
            var userId = GetUserId();

            if (!await _channelPermissionService.CanEditApprovalAsync(channelId, userId))
                return Forbid();

            var channelApprovalDetails = await _channelApprovals.GetChannelApprovalById(channelApprovalId);

            if (channelApprovalDetails == null)
                return NotFound(new { message = "Approval not found" });

            //var user = await _users.GetUserByEmailAsync(dto.ApprovalUserEmail);
            //if (user != null)
            //{     
            //    //return Forbid();

            //    var channelUser = _channelUsers.GetChannelUserAsync(channelId, user.UserId);
            //    if (channelUser != null)
            //    return Forbid();
            //}

            // HERE, WE ARE ASSUMING THAT THE USER TO BE APPROVED TO ALLOW TO JOIN THE CHANNEL HAS ALREADY JOINED THE CHANNEL, OTHERWISE AUTOMATIC ADDING THE USER TO THE CHANNEL WOULD ADD ADDITIONAL OVERHEAD OF ASSIGNING AN EXTRA PASSWORD AND NAME TO THE USER BEING AOOROVED AND PROVIDING PROVISION TO ALLOW THE USERS TO CHANGE THEIR CREDENTIALS LATER... EXTRA DEVELOPMENT OVERHEAD...

            var approvalDesc = System.Text.Json.JsonSerializer.Deserialize<ChannelApprovalDetails>(channelApprovalDetails.ApprovalDescription);
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
                UserId = user.UserId,
                Role = approvalDesc.Role
            };

            await _channelUsers.AddUserToChannelAsync(channelUserCreate);

            await _channelApprovals.UpdateChannelApprovalStatus(channelApprovalId, "approve");

            return Ok(new { message = "Member added successfully" });
        }

        [HttpGet("/Reject/{channelId}/{channelApprovalId}")]
        public async Task<IActionResult> RejectApprovals(Guid channelId, Guid channelApprovalId)
        {
            var userId = GetUserId();

            if (!await _channelPermissionService.CanEditApprovalAsync(channelId, userId))
                return Forbid();

            var channelApprovalDetails = await _channelApprovals.GetChannelApprovalById(channelApprovalId);

            if (channelApprovalDetails == null)
                return NotFound(new { message = "Approval not found" });

            await _channelApprovals.UpdateChannelApprovalStatus(channelApprovalId, "reject");

            return Ok(new { message = "Approval rejected successfully" });
        }
    }
}
