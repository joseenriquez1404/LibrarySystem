using LibrarySystem.Dtos;
using LibrarySystem.Models;
using LibrarySystem.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
        {
            var user = await authService.RegisterAsync(request);

            return StatusCode(StatusCodes.Status201Created, ToResponse(user));
        }

        [EnableRateLimiting("login")]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var (user, token, expiresAt) = await authService.LoginAsync(request);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                User = ToResponse(user)
            });
        }

        private static UserResponse ToResponse(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            Active = user.Active
        };
    }
}
