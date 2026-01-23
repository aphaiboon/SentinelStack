using Microsoft.EntityFrameworkCore;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Infrastructure.Repositories;

/// <summary>
/// Repository for EscalationPolicy entities.
/// </summary>
public class EscalationPolicyRepository : Repository<EscalationPolicy>, IEscalationPolicyRepository
{
    public EscalationPolicyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<EscalationPolicy?> GetDefaultPolicyAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(p => p.IsDefault && p.IsActive, cancellationToken);
    }

    public async Task<EscalationPolicy?> GetByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(p => p.ServiceId == serviceId && p.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<EscalationPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Steps.OrderBy(s => s.StepOrder))
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<EscalationPolicy?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
