using LibrarySystem.Data;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class AuditService(AppDbContext _context) : IAuditService
    {
        public const int DefaultLimit = 100;
        public const int MaxLimit = 500;

        // Sin paginación todavía: el límite evita devolver toda la tabla.
        public async Task<List<AuditLog>> GetRecentAsync(string? entity, int? actorId, int limit)
        {
            if (limit < 1 || limit > MaxLimit)
            {
                throw new InvalidInputException($"Limit must be between 1 and {MaxLimit}.");
            }

            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(entity))
            {
                var name = entity.Trim();
                query = query.Where(a => a.Entity == name);
            }

            if (actorId is not null)
            {
                query = query.Where(a => a.ActorId == actorId);
            }

            return await query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Id)
                .Take(limit)
                .ToListAsync();
        }
    }
}
