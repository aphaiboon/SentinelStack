using Bogus;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.UnitTests.Builders;

/// <summary>
/// Test builder for Tenant entity using Bogus.
/// </summary>
public class TenantBuilder
{
    private string? _name;
    private string? _subdomain;
    private TenantTier _tier = TenantTier.Free;
    private TenantStatus _status = TenantStatus.Active;

    private static readonly Faker Faker = new();

    public static TenantBuilder Default() => new();

    public TenantBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TenantBuilder WithSubdomain(string subdomain)
    {
        _subdomain = subdomain;
        return this;
    }

    public TenantBuilder WithTier(TenantTier tier)
    {
        _tier = tier;
        return this;
    }

    public TenantBuilder Suspended()
    {
        _status = TenantStatus.Suspended;
        return this;
    }

    public Tenant Build()
    {
        var name = _name ?? Faker.Company.CompanyName();
        var subdomain = _subdomain ?? Faker.Internet.DomainWord();

        var tenant = new Tenant(name, subdomain, _tier);

        if (_status == TenantStatus.Suspended)
        {
            tenant.Suspend();
        }

        return tenant;
    }
}
