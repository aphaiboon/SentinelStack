using FluentValidation;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Tenants.DTOs;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Tenants.Commands;

/// <summary>
/// Command to create a new tenant (FR67).
/// AC #1: Creates tenant with name, subdomain, tier.
/// </summary>
public class CreateTenantCommand
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public TenantTier Tier { get; set; }

    private readonly ITenantRepository _tenantRepository;

    public CreateTenantCommand(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<TenantDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Validate using FluentValidation
        var validator = new CreateTenantCommandValidator(_tenantRepository);
        var validationResult = await validator.ValidateAsync(this, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException($"Validation failed: {errors}");
        }

        // AC #3, #4: Auto-set isolation model based on tier
        var tenant = new Tenant(Name, Subdomain, Tier);

        await _tenantRepository.CreateAsync(tenant, cancellationToken);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            Tier = tenant.Tier,
            IsolationModel = tenant.IsolationModel,
            Status = tenant.Status,
            SchemaName = tenant.SchemaName,
            CreatedAtUtc = tenant.CreatedAtUtc,
            SuspendedAtUtc = tenant.SuspendedAtUtc,
            UpdatedAtUtc = tenant.UpdatedAtUtc
        };
    }
}

/// <summary>
/// Validator for CreateTenantCommand (AC #1).
/// Task 6: FluentValidation with DNS-safe subdomain and uniqueness check.
/// </summary>
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTenantCommandValidator(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        // AC #1: Subdomain validation (DNS-safe, 3-63 chars)
        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required")
            .Length(3, 63).WithMessage("Subdomain must be 3-63 characters")
            .Matches("^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$")
                .WithMessage("Subdomain must be lowercase alphanumeric with hyphens (DNS-safe)")
            .MustAsync(BeUniqueSubdomain)
                .WithMessage("Subdomain is already taken");

        RuleFor(x => x.Tier)
            .IsInEnum().WithMessage("Invalid tenant tier");
    }

    private async Task<bool> BeUniqueSubdomain(string subdomain, CancellationToken cancellationToken)
    {
        var existing = await _tenantRepository.GetBySubdomainAsync(subdomain, cancellationToken);
        return existing == null;
    }
}
