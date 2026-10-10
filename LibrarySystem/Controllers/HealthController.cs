using LibrarySystem.Data;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController(AppDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var canConnect = await db.Database.CanConnectAsync();

            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "unhealthy", database = "unreachable" });
            }

            return Ok(new { status = "healthy", database = "connected" });
        }
    }
}
