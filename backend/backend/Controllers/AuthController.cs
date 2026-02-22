namespace backend.Controllers
{
    using backend.Dto.Auth;
    using backend.Models;
    using backend.Repositories.Interfaces;
    using backend.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Cryptography;
    using System.Text;

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
            if (await _userRepository.IsUserExistsAsync(request.Email))
                return BadRequest(new { message = "User already exists" });

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = Convert.ToBase64String(passwordHash),
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
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new { message = "Invalid credentials" });

            if (!VerifyPasswordHash(request.Password, user.PasswordHash))
                return BadRequest(new { message = "Invalid credentials" });

            var token = _authService.GenerateToken(user);

            return Ok(new { token, user });
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            using (var hmac = new HMACSHA512())
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(hashBytes);
            }
        }
    }
}
