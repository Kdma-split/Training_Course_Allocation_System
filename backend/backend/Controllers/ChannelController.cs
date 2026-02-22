using backend.Dto;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChannelsController : ControllerBase
    {
        private readonly IChannelRepository _channelRepository;
        private readonly IChannelPermissionService _channelPermissionService;

        public ChannelsController(
            IChannelRepository channelRepository,
            IChannelPermissionService ChannelPermissionService
        )
        {
            channelRepository = channelRepository;
            _channelPermissionService = ChannelPermissionService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetChannelById(Guid channelId)
        {
            var userId = GetUserId();

            if (!await _channelPermissionService.CanCreateAsync(channelId, userId))
                return Forbid();

            var channelCourses = await _channelCourseRepository.GetCoursesByChannelAsync(channelId);
            var courses = channelCourses.Select(cc => new CourseResponseDto
            {
                Id = cc.Course.Id,
                Title = cc.Course.Title,
                Description = cc.Course.Description,
                DomainId = cc.Course.DomainId,
                NumberAttended = cc.Course.NumberAttended,
                CreatedAt = cc.Course.CreatedAt
            });

            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            if (!await _permissionService.CanReadAsync(channelId, userId))
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
                CreatedAt = course.CreatedAt
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            var userId = GetUserId();

            if (!await _permissionService.CanCRUDCourseAsync(dto.ChannelId, userId))
                return Forbid();

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                DomainId = dto.DomainId,
                NumberAttended = 0
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto dto, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            if (!await _permissionService.CanCRUDCourseAsync(channelId, userId))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id, [FromQuery] Guid channelId)
        {
            var userId = GetUserId();

            if (!await _permissionService.CanDeleteCourseAsync(channelId, userId))
                return Forbid();

            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            await _courseRepository.DeleteCourseAsync(id);

            return NoContent();
        }
    }
}
