using Microsoft.AspNetCore.Mvc;

namespace SentinelStack.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "sentinelstack-api",
            version = "1.0.0",
            timestamp = DateTime.UtcNow,
        });
    }
}