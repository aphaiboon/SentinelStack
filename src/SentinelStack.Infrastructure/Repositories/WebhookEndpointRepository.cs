using Microsoft.EntityFrameworkCore;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Infrastructure.Repositories;

/// <summary>
/// Repository for WebhookEndpoint entities.
/// </summary>
public class WebhookEndpointRepository : Repository<WebhookEndpoint>, IWebhookEndpointRepository
{
    public WebhookEndpointRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<WebhookEndpoint?> GetByEndpointKeyAsync(string endpointKey, CancellationToken cancellationToken = default)
    {
        // Bypass tenant filter for webhook receipt - we need to find the endpoint
        // regardless of which tenant is in context
        return await _context.WebhookEndpoints
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.EndpointKey == endpointKey.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookEndpoint>> GetActiveEndpointsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EndpointKeyExistsAsync(string endpointKey, CancellationToken cancellationToken = default)
    {
        // Check globally, not just within tenant
        return await _context.WebhookEndpoints
            .IgnoreQueryFilters()
            .AnyAsync(w => w.EndpointKey == endpointKey.ToLowerInvariant(), cancellationToken);
    }
}
