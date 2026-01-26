using SentinelStack.Application.Common.Interfaces;
using Serilog.Core;
using Serilog.Events;

namespace SentinelStack.Infrastructure.Logging;

/// <summary>
/// Serilog enricher that adds the current user ID to all log events.
/// Enables filtering logs by user in CloudWatch Logs Insights.
/// </summary>
public class UserEnricher : ILogEventEnricher
{
    private readonly ICurrentUserService _userService;

    public UserEnricher(ICurrentUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_userService.UserId.HasValue)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("UserId", _userService.UserId.Value));
        }
    }
}
