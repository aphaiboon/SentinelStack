using Microsoft.EntityFrameworkCore;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Infrastructure.Repositories;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Service?> GetByKeyAsync(string serviceKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = serviceKey.ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(s => s.ServiceKey == normalizedKey, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetActiveServicesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(string serviceKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = serviceKey.ToLowerInvariant();
        return await _dbSet.AnyAsync(s => s.ServiceKey == normalizedKey, cancellationToken);
    }
}
