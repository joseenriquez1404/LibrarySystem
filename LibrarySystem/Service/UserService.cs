using System.Text.Json;
using LibrarySystem.Data;
using LibrarySystem.Dtos;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class UserService(AppDbContext db) : IUserService
    {
        public async Task<UserDto> GetByIdAsync(int id)
        {
            var user = await FindUserAsync(id);

            return UserDto.From(user);
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await db.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .ToListAsync();

            return users.Select(UserDto.From).ToList();
        }

        public async Task<UserDto> ChangeRoleAsync(int actorId, int userId, Roles newRole)
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
                return UserDto.From(user);
            }

            user.Role = newRole;

            db.AuditLogs.Add(new AuditLog
            {
                ActorId = actorId,
                Action = "ChangeRole",
                Entity = "User",
                Before = JsonSerializer.Serialize(new { user.Id, Role = previousRole.ToString() }),
                After = JsonSerializer.Serialize(new { user.Id, Role = newRole.ToString() }),
                Date = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            return UserDto.From(user);
        }

        public async Task<UserDto> SetActiveAsync(int actorId, int userId, bool active)
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
                await db.SaveChangesAsync();
            }

            return UserDto.From(user);
        }

        private async Task<User> FindUserAsync(int id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

            return user ?? throw new NotFoundException("User not found.");
        }
        private async Task EnsureAdminAsync(int actorId)
        {
            var isAdmin = await db.Users.AnyAsync(u => u.Id == actorId && u.Active && u.Role == Roles.Admin);

            if (!isAdmin)
            {
                throw new ForbiddenException("Only an administrator can perform this action.");
            }
        }
    }
}
