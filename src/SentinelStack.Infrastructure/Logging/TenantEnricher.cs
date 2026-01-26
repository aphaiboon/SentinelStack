using Serilog.Core;
using Serilog.Events;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Logging;

/// <summary>
/// Serilog enricher that adds the current tenant ID to all log events.
/// Enables filtering logs by tenant in CloudWatch Logs Insights.
/// </summary>
public class TenantEnricher : ILogEventEnricher
{
    private readonly ICurrentTenantService _tenantService;

    public TenantEnricher(ICurrentTenantService tenantService)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_tenantService.TenantId.HasValue)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TenantId", _tenantService.TenantId.Value));
        }
    }
}
