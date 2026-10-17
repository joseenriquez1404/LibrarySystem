using LibrarySystem.Dtos;
using LibrarySystem.Models;
using LibrarySystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = "Admin")]
    public class AuditController(IAuditService auditService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AuditLogResponse>>> GetRecent(
            string? entity,
            int? actorId,
            int limit = AuditService.DefaultLimit)
        {
            var logs = await auditService.GetRecentAsync(entity, actorId, limit);

            return Ok(logs.Select(ToResponse).ToList());
        }

        private static AuditLogResponse ToResponse(AuditLog log) => new()
        {
            Id = log.Id,
            ActorId = log.ActorId,
            Action = log.Action,
            Entity = log.Entity,
            Before = log.Before,
            After = log.After,
            Date = log.Date
        };
    }
}
