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
        private readonly IChannelPermissionService _channelPermissionService;

        public ChannelController(
            IChannelRepository channelRepository,
            IChannelUserRepository channelUserRepository,
            IChannelPermissionService channelPermissionService)
        {
            _channelRepository = channelRepository;
            _channelUserRepository = channelUserRepository;
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
            var response = channels.Select(c => new Dto.Channel.Channel
            {
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChannel(Guid id)
        {
            var channel = await _channelRepository.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound();

            var response = new Dto.Channel.Channel
            {
                Name = channel.Name,
                Description = channel.Description,
                CreatedAt = channel.CreatedAt,
                UpdatedAt = channel.UpdatedAt
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChannel([FromBody] Dto.Channel.CreateChannelDto dto)
        {
            var userId = GetUserId();

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
                Role = Role.Author
            };
            await _channelUserRepository.AddUserToChannelAsync(channelUser);

            return CreatedAtAction(nameof(GetChannel), new { id = createdChannel.ChannelId }, createdChannel);
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

            return Ok(updatedChannel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChannel(Guid id)
        {
            var userId = GetUserId();

            if (!await _channelPermissionService.CanEditChannelInfoAsync(id, userId))
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
