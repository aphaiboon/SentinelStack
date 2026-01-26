using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Tenants.DTOs;

namespace SentinelStack.Application.Tenants.Queries;

/// <summary>
/// Query to get all tenants with pagination (AC #5).
/// </summary>
public class GetTenantsQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    private readonly ITenantRepository _tenantRepository;

    public GetTenantsQuery(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<(List<TenantDto> Items, int TotalCount)> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var (tenants, totalCount) = await _tenantRepository.GetAllAsync(Page, PageSize, cancellationToken);

        var dtos = tenants.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            Subdomain = t.Subdomain,
            Tier = t.Tier,
            IsolationModel = t.IsolationModel,
            Status = t.Status,
            SchemaName = t.SchemaName,
            CreatedAtUtc = t.CreatedAtUtc,
            SuspendedAtUtc = t.SuspendedAtUtc,
            UpdatedAtUtc = t.UpdatedAtUtc
        }).ToList();

        return (dtos, totalCount);
    }
}

/// <summary>
/// Query to get tenant by subdomain for resolution (AC #2).
/// </summary>
public class GetTenantBySubdomainQuery
{
    public string Subdomain { get; set; } = string.Empty;

    private readonly ITenantRepository _tenantRepository;

    public GetTenantBySubdomainQuery(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<TenantInfoDto?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetBySubdomainAsync(Subdomain, cancellationToken);

        if (tenant == null)
            return null;

        return new TenantInfoDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain
        };
    }
}
