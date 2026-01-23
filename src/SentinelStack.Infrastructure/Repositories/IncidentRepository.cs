using Microsoft.EntityFrameworkCore;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Infrastructure.Repositories;

public class IncidentRepository : Repository<Incident>, IIncidentRepository
{
    public IncidentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Incident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.Severity == severity)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.ServiceId == serviceId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetOpenIncidentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed)
            .OrderByDescending(i => i.Severity)
            .ThenByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.CreatedAtUtc >= startUtc && i.CreatedAtUtc <= endUtc)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
