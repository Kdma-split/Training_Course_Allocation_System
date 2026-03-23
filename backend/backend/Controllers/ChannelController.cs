namespace backend.Controllers
{
    using backend.Data;
    using backend.Models;
    using backend.Repositories.Implementations;
    using backend.Repositories.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
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
        private readonly IChannelUserRoleRepository _channelUserRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly TrainingCourseContext _context;

        public ChannelController(
            IChannelRepository channelRepository,
            IChannelUserRepository channelUserRepository,
            IChannelCourseRepository channelCourseRepository,
            IChannelAssignmentRepository channelAssignmentRepository,
            IChannelUserRoleRepository channelUserRoleRepository,
            IUserRepository userRepository,
            TrainingCourseContext context
        )
        {
            _channelRepository = channelRepository;
            _channelUserRepository = channelUserRepository;
            _channelCourseRepository = channelCourseRepository;
            _channelAssignmentRepository = channelAssignmentRepository;
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

        private async Task<bool> CanDeleteChannelAsync(Guid channelId, Guid userId)
        {
            var userRole = await _userRepository.GetUserRoleAsync(userId);
            if (userRole == "Admin")
                return true;

            return await IsChannelAdminAsync(channelId, userId);
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
            
    // CHECK IF THE USER EXISTS IN THE USER TABLE  ----  CAN'T CREATE A CHANNEL --- MUST BE A NORMAL USER OF THE PLATFORM...
            if (await _userRepository.GetUserByIdAsync(userId) == null)
                return Forbid();

            if (await _channelUserRepository.IsChannelExists(dto.Name))
                return BadRequest("Channel already exists!");

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
                UserId = userId
            };
            await _channelUserRepository.AddUserToChannelAsync(channelUser);

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "ChannelAdmin");
            if (adminRole != null)
            {
                var channelUserRole = new ChannelUserRoles
                {
                    Id = Guid.NewGuid(),
                    ChannelUserId = channelUser.ChannelUserId,
                    RoleId = adminRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.ChannelUserRoles.AddAsync(channelUserRole);
                await _context.SaveChangesAsync();
            }

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

            if (!await IsChannelAdminAsync(id, userId))
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

            if (!await CanDeleteChannelAsync(id, userId))
                return Forbid();

            var channel = await _channelRepository.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound();

            var success = await _channelRepository.SoftDeleteChannelAsync(id);
            if (!success)
                return BadRequest("Failed to delete channel");

            return NoContent();
        }
    }
}
