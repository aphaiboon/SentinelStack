using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SentinelStack.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var process = Process.GetCurrentProcess();
        var startTimeUtc = process.StartTime.ToUniversalTime();
        var uptime = DateTime.UtcNow - startTimeUtc;

        double cpuPercentSinceStart = 0.0;
        try
        {
            var totalMs = process.TotalProcessorTime.TotalMilliseconds;
            var uptimeMs = Math.Max(1, uptime.TotalMilliseconds);
            cpuPercentSinceStart = (totalMs / uptimeMs) / Environment.ProcessorCount * 100.0;
        }
        catch
        {
            // best-effort: if anything goes wrong, leave cpuPercentSinceStart as 0
        }

        var memoryWorkingSetMb = process.WorkingSet64 / 1024.0 / 1024.0;
        var privateMemoryMb = process.PrivateMemorySize64 / 1024.0 / 1024.0;
        var gcAllocatedMb = GC.GetTotalMemory(false) / 1024.0 / 1024.0;

        return Ok(new
        {
            status = "ok",
            service = "sentinelstack-api",
            version = "1.0.0",
            timestamp = DateTime.UtcNow,
            uptime = uptime.ToString(@"dd\.hh\:mm\:ss"),
            uptimeSeconds = (long)uptime.TotalSeconds,
            pid = process.Id,
            threadCount = process.Threads.Count,
            cpuPercentSinceStart = Math.Round(cpuPercentSinceStart, 2),
            memory = new
            {
                workingSetMb = Math.Round(memoryWorkingSetMb, 2),
                privateMb = Math.Round(privateMemoryMb, 2),
                gcAllocatedMb = Math.Round(gcAllocatedMb, 2)
            }
        });
    }
}