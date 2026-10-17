using LibrarySystem.Models;

namespace LibrarySystem.Service
{
    public interface IAuditService
    {
        Task<List<AuditLog>> GetRecentAsync(string? entity, int? actorId, int limit);
    }
}
