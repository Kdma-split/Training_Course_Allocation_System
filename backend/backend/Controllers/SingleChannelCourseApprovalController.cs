using backend.Data;
using backend.Dto;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SingleChannelCourseApprovalController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IChannelCourseRepository _channelCourseRepository;
        private readonly IChannelUserRoleRepository _channelUserRoleRepository;
        private readonly ICourseApprovalRepository _courseApprovalRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseChannelUserRepository _courseChannelUserRepository;

        public SingleChannelCourseApprovalController(
            ICourseRepository courseRepository,
            IChannelCourseRepository channelCourseRepository,
            IChannelUserRoleRepository channelUserRoleRepository,
            ICourseApprovalRepository courseApprovalRepository,
            IUserRepository userRepository,
            ICourseChannelUserRepository courseChannelUserRepository
         )
        {
            _courseRepository = courseRepository;
            _channelCourseRepository = channelCourseRepository;
            _channelUserRoleRepository = channelUserRoleRepository;
            _courseApprovalRepository = courseApprovalRepository; 
            _userRepository = userRepository;
            _courseChannelUserRepository = courseChannelUserRepository;
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

        private async Task<bool> IsChannelAdminAsync(Guid channelId, Guid userId)
        {
            var roles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            return roles != null && roles.Contains("ChannelAdmin");
        }

        private async Task<bool> CanManageCourseAsync(Guid channelId, Guid userId)
        {
            var roles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            if (roles == null) return false;
            return roles.Contains("ChannelAdmin") || roles.Contains("CourseAdmin") || roles.Contains("CourseEditor");
        }

        private async Task<bool> IsCourseAuthorOrAdminAsync(Guid channelId, Guid courseId, Guid userId)
        {
            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null) return false;

            var roles = await _channelUserRoleRepository.GetUserRoleInChannelAsync(channelId, userId);
            if (roles == null) return false;

            if (roles.Contains("ChannelAdmin") || roles.Contains("CourseAdmin"))
                return true;

            return course.CreatedBy == userId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateApproval([FromBody] CreateCourseApprovalDto dto)
        {
            var userId = GetUserId();

            var approvalDesc = JsonSerializer.Deserialize<ApprovalDescriptionDto>(dto.ApprovalDescription);
            if (approvalDesc == null)
                return BadRequest("Invalid approval description format");

            var channelId = approvalDesc.ChannelId;

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            if (approvalDesc.Category.ToLower() == "create")
            {
                if (!await CanManageCourseAsync(channelId, userId))
                    return Forbid("Only authors, editors, or admins can create approvals");
            }
            else if (approvalDesc.Category.ToLower() == "update")
            {
                if (approvalDesc.Details.CourseId == null)
                    return BadRequest("CourseId is required for update approvals");

                var channelCourse = await _channelCourseRepository.GetChannelCourseAsync(channelId, approvalDesc.Details.CourseId.Value);
                if (channelCourse == null)
                    return NotFound("Course does not exist in this channel");

                if (!await IsCourseAuthorOrAdminAsync(channelId, approvalDesc.Details.CourseId.Value, userId))
                    return Forbid("You don't have permission to seek approval for this course update");
            }
            else
            {
                return BadRequest("Invalid category. Must be 'create' or 'update'");
            }

            var courseChannelUser = await _courseChannelUserRepository.GetByUserIdAndChannelIdAsync(userId, channelId);

            if (courseChannelUser == null)
            {
                var channelCourse = await _channelCourseRepository.GetFirstChannelCourseByChannelIdAsync(channelId);
                
                if (channelCourse == null)
                    return NotFound("Channel not found");

                courseChannelUser = new CourseChannelUser
                {
                    UserId = userId,
                    ChannelCourseId = channelCourse.ChannelCourseId,
                    Role = "Author",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                courseChannelUser = await _courseChannelUserRepository.CreateAsync(courseChannelUser);
            }

            var approval = new CourseApproval
            {
                CourseChannelUserId = courseChannelUser.CourseChannelUserId,
                ApprovalDescription = dto.ApprovalDescription,
                Status = "pending",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdApproval = await _courseApprovalRepository.CreateCourseApprovalAsync(approval);

            return CreatedAtAction(nameof(GetApproval), new { id = createdApproval.CourseApprovalId }, createdApproval);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApproval(Guid id)
        {
            var approval = await _courseApprovalRepository.GetCourseApprovalByIdAsync(id);
            if (approval == null)
                return NotFound();

            return Ok(new CourseApprovalResponseDto
            {
                CourseApprovalId = approval.CourseApprovalId,
                UserId = approval.CourseChannelUser.UserId,
                ApprovalDescription = approval.ApprovalDescription,
                Status = approval.Status,
                IsActive = approval.IsActive,
                CreatedAt = approval.CreatedAt,
                UpdatedAt = approval.UpdatedAt
            });
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveApproval(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only admins can approve approvals");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approval = await _courseApprovalRepository.GetCourseApprovalByIdAsync(id);
            if (approval == null)
                return NotFound();

            if (approval.Status != "pending")
                return BadRequest("Cannot re-acknowledge an already acknowledged approval");

            var success = await _courseApprovalRepository.UpdateCourseApprovalStatusAsync(id, "approved");
            if (!success)
                return BadRequest("Failed to approve approval");

            return Ok(new { message = "Approval approved successfully" });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectApproval(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only admins can reject approvals");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approval = await _courseApprovalRepository.GetCourseApprovalByIdAsync(id);
            if (approval == null)
                return NotFound();

            if (approval.Status != "pending")
                return BadRequest("Cannot re-acknowledge an already acknowledged approval");

            var success = await _courseApprovalRepository.UpdateCourseApprovalStatusAsync(id, "rejected");
            if (!success)
                return BadRequest("Failed to reject approval");

            return Ok(new { message = "Approval rejected successfully" });
        }

        [HttpGet("all")]
        public async Task<IActionResult> ListAllApprovals([FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only admins can view all approvals");

            var approvals = await _courseApprovalRepository.GetAllApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> ListPendingApproval([FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only admins can view pending approvals");

            var approvals = await _courseApprovalRepository.GetPendingApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("approved")]
        public async Task<IActionResult> ListApprovedApprovals([FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only admins can view approved approvals");

            var approvals = await _courseApprovalRepository.GetApprovedApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("rejected")]
        public async Task<IActionResult> ListRejectedApprovals([FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only admins can view rejected approvals");

            var approvals = await _courseApprovalRepository.GetRejectedApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("my")]
        public async Task<IActionResult> ListUserApprovalsChannel([FromQuery] Guid? channelId)
        {
            var userId = GetUserId();

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (channelId.HasValue)
            {
                if (!await IsChannelMemberAsync(channelId.Value, userId))
                    return Forbid("User is not a member of this channel");

                var approvs = await _courseApprovalRepository.GetAllApprovalsByUserIdAsync(userId);

                var filteredApprovs = approvs.Where(a =>
                    a.CourseChannelUser.ChannelCourse != null &&
                    a.CourseChannelUser.ChannelCourse.ChannelId == channelId);

                var res = filteredApprovs.Select(a => new CourseApprovalResponseDto
                {
                    CourseApprovalId = a.CourseApprovalId,
                    UserId = a.CourseChannelUser.UserId,
                    ApprovalDescription = a.ApprovalDescription,
                    Status = a.Status,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                });

                return Ok(res);
            }

            var approvals = await _courseApprovalRepository.GetAllApprovalsByUserIdAsync(userId);

            var filteredApprovals = approvals.Where(a =>
                a.CourseChannelUser.ChannelCourse != null);

            var response = filteredApprovals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });
            return Ok(response);
        }

        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetApprovalsByChannel(Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetAllApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/course/{courseId}")]
        public async Task<IActionResult> GetApprovalsByChannelCourse(Guid channelId, Guid courseId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetAllApprovalsByChannelCourseAsync(channelId, courseId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/user/{memberId}")]
        public async Task<IActionResult> GetApprovalsByChannelUser(Guid channelId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetApprovalsByChannelIdUserIdAsync(channelId, memberId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/course/{courseId}/user/{memberId}")]
        public async Task<IActionResult> GetApprovalsByChannelCourseUser(Guid channelId, Guid courseId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetAllApprovalsByChannelCourseUserAsync(channelId, courseId, memberId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/pending")]
        public async Task<IActionResult> GetPendingApprovalsByChannel(Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetPendingApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/course/{courseId}/pending")]
        public async Task<IActionResult> GetPendingApprovalsByChannelCourse(Guid channelId, Guid courseId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetPendingApprovalsByChannelCourseAsync(channelId, courseId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/user/{memberId}/pending")]
        public async Task<IActionResult> GetPendingApprovalsByChannelUser(Guid channelId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetPendingApprovalsByChannelCourseUserAsync(channelId, memberId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/approved")]
        public async Task<IActionResult> GetApprovedApprovalsByChannel(Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetApprovedApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/course/{courseId}/approved")]
        public async Task<IActionResult> GetApprovedApprovalsByChannelCourse(Guid channelId, Guid courseId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetApprovedApprovalsByChannelCourseAsync(channelId, courseId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/user/{memberId}/approved")]
        public async Task<IActionResult> GetApprovedApprovalsByChannelUser(Guid channelId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetApprovedApprovalsByChannelIdUserIdAsync(channelId, memberId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/course/{courseId}/user/{memberId}/approved")]
        public async Task<IActionResult> GetApprovedApprovalsByChannelCourseUser(Guid channelId, Guid courseId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetApprovedApprovalsByChannelCourseAsync(channelId, courseId, memberId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/rejected")]
        public async Task<IActionResult> GetRejectedApprovalsByChannel(Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetRejectedApprovalsByChannelAsync(channelId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/course/{courseId}/rejected")]
        public async Task<IActionResult> GetRejectedApprovalsByChannelCourse(Guid channelId, Guid courseId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetRejectedApprovalsByChannelCourseAsync(channelId, courseId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/user/{memberId}/rejected")]
        public async Task<IActionResult> GetRejectedApprovalsByChannelUser(Guid channelId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetRejectedApprovalsByChannelIdUserIdAsync(channelId, memberId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("channel/{channelId}/course/{courseId}/user/{memberId}/rejected")]
        public async Task<IActionResult> GetRejectedApprovalsByChannelCourseUser(Guid channelId, Guid courseId, Guid memberId)
        {
            var userId = GetUserId();

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetRejectedApprovalsByChannelCourseUserAsync(channelId, courseId, memberId);
            var response = approvals.Select(a => new CourseApprovalResponseDto
            {
                CourseApprovalId = a.CourseApprovalId,
                UserId = a.CourseChannelUser.UserId,
                ApprovalDescription = a.ApprovalDescription,
                Status = a.Status,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApproval(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            if (!await IsChannelAdminAsync(channelId, userId))
                return Forbid("Only admins can delete approvals");

            var approval = await _courseApprovalRepository.GetCourseApprovalByIdAsync(id);
            if (approval == null)
                return NotFound();

            var success = await _courseApprovalRepository.DeleteCourseApprovalAsync(id);
            if (!success)
                return BadRequest("Failed to delete approval");

            return Ok(new { message = "Approval deleted successfully" });
        }
    }
}
