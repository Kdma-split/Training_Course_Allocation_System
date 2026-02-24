namespace backend.Controllers
{
    using backend.Dto.Auth;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using backend.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public AuthController(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and Password are required" });

            if (await _userRepository.IsUserExistsAsync(request.Email))
                return BadRequest(new { message = "User already exists" });

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Age = request.Age,
                Role = request.Role ?? "User"
            };

            await _userRepository.CreateUserAsync(user);

            var token = _authService.GenerateToken(user);

            return Ok(new { token, user });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and Password are required" });

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new { message = "Invalid credentials" });

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return BadRequest(new { message = "Invalid credentials" });

            var token = _authService.GenerateToken(user);

            return Ok(new { token, user });
        }
    }
}
