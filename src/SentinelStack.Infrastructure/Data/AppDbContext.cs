using Microsoft.EntityFrameworkCore;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Common;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Infrastructure.Data;

/// <summary>
/// Main database context with multi-tenant support.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentTenantService? _currentTenantService;
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EscalationPolicy> EscalationPolicies => Set<EscalationPolicy>();
    public DbSet<EscalationStep> EscalationSteps => Set<EscalationStep>();
    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply global query filter for tenant isolation
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplyTenantQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object?[] { modelBuilder, _currentTenantService });
            }
        }
    }

    private static void ApplyTenantQueryFilter<T>(ModelBuilder modelBuilder, ICurrentTenantService? tenantService)
        where T : class, ITenantEntity
    {
        if (tenantService?.TenantId != null)
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == tenantService.TenantId);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (_dateTimeProvider != null)
                    {
                        entry.Property(nameof(AuditableEntity.CreatedAtUtc)).CurrentValue = _dateTimeProvider.UtcNow;
                    }
                    if (_currentUserService?.UserId != null)
                    {
                        entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = _currentUserService.UserId;
                    }
                    break;

                case EntityState.Modified:
                    if (_dateTimeProvider != null)
                    {
                        entry.Property(nameof(AuditableEntity.UpdatedAtUtc)).CurrentValue = _dateTimeProvider.UtcNow;
                    }
                    if (_currentUserService?.UserId != null)
                    {
                        entry.Property(nameof(AuditableEntity.UpdatedBy)).CurrentValue = _currentUserService.UserId;
                    }
                    break;
            }
        }

        // Dispatch domain events
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after save
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }
}
