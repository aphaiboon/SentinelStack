using Microsoft.EntityFrameworkCore;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Infrastructure.Repositories;

public class SamlConfigurationRepository : ISamlConfigurationRepository
{
    private readonly AppDbContext _context;

    public SamlConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SamlConfiguration?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.SamlConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
    }

    public async Task<SamlConfiguration?> GetByIdpEntityIdAsync(string idpEntityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idpEntityId))
        {
            return null;
        }

        return await _context.SamlConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdpEntityId == idpEntityId, cancellationToken);
    }

    public async Task<SamlConfiguration> CreateOrUpdateAsync(SamlConfiguration samlConfiguration, CancellationToken cancellationToken = default)
    {
        // Check if config already exists for this tenant
        var existing = await _context.SamlConfigurations
            .FirstOrDefaultAsync(s => s.TenantId == samlConfiguration.TenantId, cancellationToken);

        if (existing != null)
        {
            // Update existing configuration
            _context.Entry(existing).State = EntityState.Detached;
            _context.SamlConfigurations.Update(samlConfiguration);
        }
        else
        {
            // Create new configuration
            await _context.SamlConfigurations.AddAsync(samlConfiguration, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return samlConfiguration;
    }

    public async Task DeleteAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var config = await _context.SamlConfigurations
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        if (config != null)
        {
            _context.SamlConfigurations.Remove(config);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
