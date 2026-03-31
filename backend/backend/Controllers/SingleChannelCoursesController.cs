//  THIS API WILL BE CALLED IN THE CASE OF MAKING A COURSE IN COLLABORATION WITH SINGLE CHANNEL ONLY [AS, ROLE WITHIN A SINGLE CHANNEL MATTER IN THIS CASE, ALL USER WILL HAVE A FIXED ROLE AS IN THE ChannelUser DB TABLE]...


using backend.Data;
using backend.Dto;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SingleChannelCoursesController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IChannelCourseRepository _channelCourseRepository;
        private readonly IChannelUserRoleRepository _channelUserRoleRepository;
        private readonly IChannelUserRepository _channelUserRepository;
        private readonly ICourseApprovalRepository _courseApprovalRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseChannelUserRepository _courseChannelUserRepository;

        public SingleChannelCoursesController(
            ICourseRepository courseRepository,
            IChannelCourseRepository channelCourseRepository,
            IChannelUserRoleRepository channelUserRoleRepository,
            IChannelUserRepository channelUserRepository,
            ICourseApprovalRepository courseApprovalRepository,
            IUserRepository userRepository,
            ICourseChannelUserRepository courseChannelUserRepository)
        {
            _courseRepository = courseRepository;
            _channelCourseRepository = channelCourseRepository;
            _channelUserRoleRepository = channelUserRoleRepository;
            _channelUserRepository = channelUserRepository;
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

        // VIEW - Any channel member can view courses
        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetCoursesByChannel(Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid();

            var channelCourses = await _channelCourseRepository.GetCoursesByChannelAsync(channelId);
            var courses = channelCourses.Select(cc => new CourseResponseDto
            {
                Id = cc.Course.Id,
                Title = cc.Course.Title,
                Description = cc.Course.Description,
                DomainId = cc.Course.DomainId,
                NumberAttended = cc.Course.NumberAttended,
                CreatedAt = cc.Course.CreatedAt,
                CreatedBy = cc.Course.CreatedBy
            });

            return Ok(courses);
        }

        // VIEW - Get single course
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid();

            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            var response = new CourseResponseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                DomainId = course.DomainId,
                NumberAttended = course.NumberAttended,
                CreatedAt = course.CreatedAt,
                CreatedBy = course.CreatedBy
            };

            return Ok(response);
        }

        // CREATE - CourseAuthor can create courses
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            var userId = GetUserId();
            
            if (!await CanManageCourseAsync(dto.ChannelId, userId))
                return Forbid();

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                DomainId = dto.DomainId,
                NumberAttended = 0,
                CreatedBy = userId
            };

            var createdCourse = await _courseRepository.CreateCourseAsync(course);

            var channelCourse = new ChannelCourse
            {
                ChannelId = dto.ChannelId,
                CourseId = createdCourse.Id,
                IsActive = true
            };

            await _channelCourseRepository.AddCourseToChannelAsync(channelCourse);

            return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id, channelId = dto.ChannelId }, createdCourse);
        }

        // UPDATE - CourseAuthor can update own courses, CourseAdmin can update any course
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto dto, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await IsCourseAuthorOrAdminAsync(channelId, id, userId))
                return Forbid();

            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.DomainId = dto.DomainId;
            course.UpdatedAt = DateTime.UtcNow;

            var updatedCourse = await _courseRepository.UpdateCourseAsync(course);

            return Ok(updatedCourse);
        }

        // DELETE - CourseAuthor can delete own courses, CourseAdmin can delete any course
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();
            
            if (!await IsCourseAuthorOrAdminAsync(channelId, id, userId))
                return Forbid();

            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            await _courseRepository.DeleteCourseAsync(id);

            return NoContent();
        }

        // APPROVAL - Create a new course approval
        [HttpPost("approval")]
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

        // APPROVAL - Get single approval
        [HttpGet("approval/{id}")]
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

        // APPROVAL - Approve an approval (Admin only)
        [HttpPut("approval/{id}/approve")]
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

        // APPROVAL - Reject an approval (Admin only)
        [HttpPut("approval/{id}/reject")]
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

        // APPROVAL - List all approvals (Admin only)
        [HttpGet("approvals/all")]
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

        // APPROVAL - List pending approvals (Admin only)
        [HttpGet("approvals/pending")]
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

        // APPROVAL - List approved approvals (Admin only)
        [HttpGet("approvals/approved")]
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

        // APPROVAL - List rejected approvals (Admin only)
        [HttpGet("approvals/rejected")]
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

        // APPROVAL - List user's own approvals
        [HttpGet("approvals/my")]
        public async Task<IActionResult> ListUserApprovals([FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!await IsChannelMemberAsync(channelId, userId))
                return Forbid("User is not a member of this channel");

            var approvals = await _courseApprovalRepository.GetAllApprovalsByUserIdAsync(userId);
            
            var filteredApprovals = approvals.Where(a => 
                a.CourseChannelUser.ChannelCourse != null && 
                a.CourseChannelUser.ChannelCourse.ChannelId == channelId);

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
    }
}
