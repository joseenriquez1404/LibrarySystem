using System.Text.Json;
using LibrarySystem.Data;
using LibrarySystem.Dtos;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class UserService(AppDbContext _context, IPasswordHasher<User> _hasher) : IUserService
    {
        public async Task<User> GetByIdAsync(int id)
        {
            var user = await FindUserAsync(id);

            return user;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(int actorId, CreateUserRequest request)
        {
            await EnsureAdminAsync(actorId);

            if (!Enum.IsDefined(request.Role))
            {
                throw new InvalidInputException("Role is not valid.");
            }

            var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new ConflictException("A user with that email already exists.");
            }

            var user = new User { Name = request.Name.Trim(), Email = email, Role = request.Role };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);

            // El Id del usuario nace al guardar; la transacción deja usuario y auditoría juntos.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                // Dos altas simultáneas con el mismo email: gana el índice único.
                throw new ConflictException("A user with that email already exists.");
            }

            AddAudit(actorId, "CreateUser", null, new { user.Id, user.Email, Role = user.Role.ToString() });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return user;
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

            AddAudit(actorId, "ChangeRole",
                new { user.Id, Role = previousRole.ToString() },
                new { user.Id, Role = newRole.ToString() });

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
                var previousActive = user.Active;
                user.Active = active;

                AddAudit(actorId, "SetActive", new { user.Id, Active = previousActive }, new { user.Id, Active = active });

                await _context.SaveChangesAsync();
            }

            return user;
        }

        // Nunca pasar contraseñas ni hashes en before/after.
        private void AddAudit(int actorId, string action, object? before, object? after)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                ActorId = actorId,
                Action = action,
                Entity = "User",
                Before = before is null ? null : JsonSerializer.Serialize(before),
                After = after is null ? null : JsonSerializer.Serialize(after),
                Date = DateTime.UtcNow
            });
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
