using backend.Dto;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SingleChannelAssignmentsController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IChannelAssignmentRepository _channelAssignmentRepository;
        private readonly IChannelUserRoleRepository _channelUserRoleRepository;

        public SingleChannelAssignmentsController(
            IAssignmentRepository assignmentRepository,
            IChannelAssignmentRepository channelAssignmentRepository,
            IChannelUserRoleRepository channelUserRoleRepository)
        {
            _assignmentRepository = assignmentRepository;
            _channelAssignmentRepository = channelAssignmentRepository;
            _channelUserRoleRepository = channelUserRoleRepository;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        private async Task<bool> IsChannelMemberAsync(Guid channelId, Guid userId)
        {
            var roles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            return roles != null;
        }

        private async Task<bool> CanManageCourseAsync(Guid channelId, Guid userId)
        {
            var roles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            if (roles == null) return false;
            return roles.Contains("ChannelAdmin") || roles.Contains("AssignmentAdmin") || roles.Contains("AssignmentEditor");
        }

        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetAssignmentsByChannel(Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid();

            var channelAssignments = await _channelAssignmentRepository.GetAssignmentsByChannelAsync(channelId);
            var assignments = channelAssignments.Select(ca => new AssignmentResponseDto
            {
                Id = ca.Assignment.Id,
                Domain = ca.Assignment.Domain,
                NumberAttended = ca.Assignment.NumberAttended,
                AssignedAt = ca.Assignment.AssignedAt
            });

            return Ok(assignments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignment(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid();

            var assignment = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound();

            var response = new AssignmentResponseDto
            {
                Id = assignment.Id,
                Domain = assignment.Domain,
                NumberAttended = assignment.NumberAttended,
                AssignedAt = assignment.AssignedAt
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto dto)
        {
            var userId = GetUserId();
            
            if (!await CanManageCourseAsync(dto.ChannelId, userId))
                return Forbid();

            var assignment = new Assignment
            {
                Domain = dto.Domain,
                NumberAttended = 0
            };

            var createdAssignment = await _assignmentRepository.CreateAssignmentAsync(assignment);

            var channelAssignment = new ChannelAssignment
            {
                ChannelId = dto.ChannelId,
                AssignmentId = createdAssignment.Id,
                CourseId = dto.CourseId,
                IsActive = true
            };

            await _channelAssignmentRepository.AddAssignmentToChannelAsync(channelAssignment);

            return CreatedAtAction(nameof(GetAssignment), new { id = createdAssignment.Id, channelId = dto.ChannelId }, createdAssignment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] UpdateAssignmentDto dto, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await CanManageCourseAsync(channelId, userId))
                return Forbid();

            var assignment = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound();

            assignment.Domain = dto.Domain;
            assignment.NumberAttended = assignment.NumberAttended;

            var updatedAssignment = await _assignmentRepository.UpdateAssignmentAsync(assignment);

            return Ok(updatedAssignment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await CanManageCourseAsync(channelId, userId))
                return Forbid();

            var assignment = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound();

            await _assignmentRepository.DeleteAssignmentAsync(id);

            return NoContent();
        }
    }
}
