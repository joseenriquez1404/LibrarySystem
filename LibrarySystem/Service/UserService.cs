using System.Text.Json;
using LibrarySystem.Data;
using LibrarySystem.Dtos;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class UserService(AppDbContext _context) : IUserService
    {
        public async Task<User> GetByIdAsync(int id)
        {
            var user = await FindUserAsync(id);

            return user;
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    Active = u.Active
                })
                .ToListAsync();
        }

        public async Task<User> ChangeRoleAsync(int actorId, int userId, Roles newRole)
        {
            await EnsureAdminAsync(actorId);

            if (!Enum.IsDefined(newRole))
            {
                throw new InvalidInputException("Role is not valid.");
            }

            if (actorId == userId)
            {
                throw new ForbiddenException("You cannot change your own role.");
            }

            var user = await FindUserAsync(userId);
            var previousRole = user.Role;

            if (previousRole == newRole)
            {
                return user;
            }

            user.Role = newRole;

            _context.AuditLogs.Add(new AuditLog
            {
                ActorId = actorId,
                Action = "ChangeRole",
                Entity = "User",
                Before = JsonSerializer.Serialize(new { user.Id, Role = previousRole.ToString() }),
                After = JsonSerializer.Serialize(new { user.Id, Role = newRole.ToString() }),
                Date = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> SetActiveAsync(int actorId, int userId, bool active)
        {
            await EnsureAdminAsync(actorId);

            if (actorId == userId && !active)
            {
                throw new ForbiddenException("You cannot deactivate your own account.");
            }

            var user = await FindUserAsync(userId);

            if (user.Active != active)
            {
                user.Active = active;
                await _context.SaveChangesAsync();
            }

            return user;
        }

        private async Task<User> FindUserAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            return user ?? throw new NotFoundException("User not found.");
        }

        private async Task EnsureAdminAsync(int actorId)
        {
            var isAdmin = await _context.Users.AnyAsync(u => u.Id == actorId && u.Active && u.Role == Roles.Admin);

            if (!isAdmin)
            {
                throw new ForbiddenException("Only an administrator can perform this action.");
            }
        }
    }
}
