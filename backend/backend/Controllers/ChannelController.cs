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
    public class ChannelController : ControllerBase
    {
        private readonly IChannelRepository _channelRepository;
        private readonly IChannelUserRepository _channelUserRepository;
        private readonly IChannelCourseRepository _channelCourseRepository;
        private readonly IChannelAssignmentRepository _channelAssignmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChannelPermissionService _channelPermissionService;

        public ChannelController(
            IChannelRepository channelRepository,
            IChannelUserRepository channelUserRepository,
            IChannelCourseRepository channelCourseRepository,
            IChannelAssignmentRepository channelAssignmentRepository,
            IUserRepository userRepository,
            IChannelPermissionService channelPermissionService
        )
        {
            _channelRepository = channelRepository;
            _channelUserRepository = channelUserRepository;
            _channelCourseRepository = channelCourseRepository;
            _channelAssignmentRepository = channelAssignmentRepository;
            _userRepository = userRepository;
            _channelPermissionService = channelPermissionService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChannels()
        {
            var channels = await _channelRepository.GetAllChannelsAsync();
            var response = new List<Dto.Channel.Channel>();

            foreach (var c in channels)
            {
                var channelUsers = await _channelUserRepository.GetUsersByChannelAsync(c.ChannelId);
                var channelCourses = await _channelCourseRepository.GetCoursesByChannelAsync(c.ChannelId);
                var channelAssignments = await _channelAssignmentRepository.GetAssignmentsByChannelAsync(c.ChannelId);

                response.Add(new Dto.Channel.Channel
                {
                    ChannelId = c.ChannelId,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ChannelUsers = channelUsers.Select(cu => cu.UserId.ToString()).ToList(),
                    ChannelCourses = channelCourses.Select(cc => cc.CourseId.ToString()).ToList(),
                    ChannelAssignments = channelAssignments.Select(ca => ca.AssignmentId.ToString()).ToList()
                });
            }

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChannel(Guid id)
        {
            var channel = await _channelRepository.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound();

            var channelUsers = await _channelUserRepository.GetUsersByChannelAsync(id);
            var channelCourses = await _channelCourseRepository.GetCoursesByChannelAsync(id);
            var channelAssignments = await _channelAssignmentRepository.GetAssignmentsByChannelAsync(id);

            var response = new Dto.Channel.Channel
            {
                ChannelId = channel.ChannelId,
                Name = channel.Name,
                Description = channel.Description,
                CreatedAt = channel.CreatedAt,
                UpdatedAt = channel.UpdatedAt,
                ChannelUsers = channelUsers.Select(cu => cu.UserId.ToString()).ToList(),
                ChannelCourses = channelCourses.Select(cc => cc.CourseId.ToString()).ToList(),
                ChannelAssignments = channelAssignments.Select(ca => ca.AssignmentId.ToString()).ToList()
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChannel([FromBody] Dto.Channel.CreateChannelDto dto)
        {
            var userId = GetUserId();
            
    // CHECK IF THE USER EXISTS IN THE USER TABLE  ----  INVOLVED IN THE PLATFORM...
            if (await _userRepository.GetUserByIdAsync(userId) != null)
                return Forbid();

            var channel = new Channel
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedById = userId,
                AdminId = userId
            };

            var createdChannel = await _channelRepository.CreateChannelAsync(channel);

            var channelUser = new ChannelUser
            {
                ChannelId = createdChannel.ChannelId,
                UserId = userId,
                Role = Role.Admin
            };
            await _channelUserRepository.AddUserToChannelAsync(channelUser);

            var response = new Dto.Channel.Channel
            {
                ChannelId = createdChannel.ChannelId,
                Name = createdChannel.Name,
                Description = createdChannel.Description,
                CreatedAt = createdChannel.CreatedAt,
                UpdatedAt = createdChannel.UpdatedAt,
                ChannelUsers = new List<string> { userId.ToString() },
                ChannelCourses = new List<string>(),
                ChannelAssignments = new List<string>()
            };

            return CreatedAtAction(nameof(GetChannel), new {
                id = createdChannel.ChannelId 
            }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChannel(Guid id, [FromBody] Dto.Channel.CreateChannelDto dto)
        {
            var userId = GetUserId();

            if (!await _channelPermissionService.CanEditChannelInfoAsync(id, userId))
                return Forbid();

            var channel = await _channelRepository.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound();

            channel.Name = dto.Name;
            channel.Description = dto.Description;
            channel.UpdatedAt = DateTime.UtcNow;

            var updatedChannel = await _channelRepository.UpdateChannelAsync(channel);

            var channelUsers = await _channelUserRepository.GetUsersByChannelAsync(id);
            var channelCourses = await _channelCourseRepository.GetCoursesByChannelAsync(id);
            var channelAssignments = await _channelAssignmentRepository.GetAssignmentsByChannelAsync(id);

            var response = new Dto.Channel.Channel
            {
                ChannelId = updatedChannel.ChannelId,
                Name = updatedChannel.Name,
                Description = updatedChannel.Description,
                CreatedAt = updatedChannel.CreatedAt,
                UpdatedAt = updatedChannel.UpdatedAt,
                ChannelUsers = channelUsers.Select(cu => cu.UserId.ToString()).ToList(),
                ChannelCourses = channelCourses.Select(cc => cc.CourseId.ToString()).ToList(),
                ChannelAssignments = channelAssignments.Select(ca => ca.AssignmentId.ToString()).ToList()
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChannel(Guid id)
        {
            var userId = GetUserId();

            if (await _channelPermissionService.CanEditChannelInfoAsync(id, userId))
                return Forbid();

            var channel = await _channelRepository.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound();

            if (channel.CreatedById != userId)
                return Forbid();

            await _channelRepository.DeleteChannelAsync(id);

            return NoContent();
        }
    }
}
