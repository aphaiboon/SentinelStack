using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Incident entities.
/// </summary>
public interface IIncidentRepository : IRepository<Incident>
{
    /// <summary>
    /// Gets incidents by status for the current tenant.
    /// </summary>
    Task<IReadOnlyList<Incident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets incidents by severity for the current tenant.
    /// </summary>
    Task<IReadOnlyList<Incident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets incidents by service ID for the current tenant.
    /// </summary>
    Task<IReadOnlyList<Incident>> GetByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets open (non-closed, non-resolved) incidents for the current tenant.
    /// </summary>
    Task<IReadOnlyList<Incident>> GetOpenIncidentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets incidents created within a date range for the current tenant.
    /// </summary>
    Task<IReadOnlyList<Incident>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
}
