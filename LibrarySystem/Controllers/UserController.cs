using System.Security.Claims;
using LibrarySystem.Dtos;
using LibrarySystem.Models;
using LibrarySystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<ActionResult<UserResponse>> GetMe()
        {
            var user = await userService.GetByIdAsync(GetActorId());

            return Ok(ToResponse(user));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            var users = await userService.GetAllAsync();

            return Ok(users.Select(ToResponse).ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var user = await userService.GetByIdAsync(id);

            return Ok(ToResponse(user));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
        {
            var user = await userService.CreateAsync(GetActorId(), request);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResponse(user));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/role")]
        public async Task<ActionResult<UserResponse>> ChangeRole(int id, ChangeRoleRequest request)
        {
            var user = await userService.ChangeRoleAsync(GetActorId(), id, request.Role);

            return Ok(ToResponse(user));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/active")]
        public async Task<ActionResult<UserResponse>> SetActive(int id, SetActiveRequest request)
        {
            var user = await userService.SetActiveAsync(GetActorId(), id, request.Active);

            return Ok(ToResponse(user));
        }

        // JwtBearer renombra el claim "sub" a NameIdentifier (MapInboundClaims está activo).
        private int GetActorId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
