using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using API.Repositories;
using API.Controllers;

[ApiController]
[Route("api/web/v1/[controller]")]
public class HealthController : BaseController
{
    private readonly IUserContext _userContext;
    private readonly IProjectContext _projectContext;

    public HealthController(
        IUserContext userContext,
        IProjectContext projectContext)
    {
        _userContext = userContext;
        _projectContext = projectContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Basic health check
            var basicHealth = new { status = "healthy", timestamp = System.DateTime.UtcNow };

            // Check database connections
            var dbHealth = await CheckDatabaseConnections();

            return Ok(new
            {
                status = "healthy",
                timestamp = System.DateTime.UtcNow,
                version = GetType().Assembly.GetName().Version?.ToString(),
                database = dbHealth
            });
        }
        catch
        {
            return StatusCode(503, new { status = "unhealthy" });
        }
    }

    [HttpGet("aws")]
    public IActionResult GetAwsHealth()
    {
        // AWS ELB expects 200 for healthy instances
        return Ok(new { status = "healthy" });
    }

    private async Task<object> CheckDatabaseConnections()
    {
        try
        {
            // Try a simple database operation
            await _userContext.Users.ReadAll();
            await _projectContext.Projects.ReadAll();

            return new { status = "connected" };
        }
        catch
        {
            return new { status = "disconnected" };
        }
    }
}