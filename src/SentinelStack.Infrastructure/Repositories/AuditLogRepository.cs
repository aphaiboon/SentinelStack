using Microsoft.EntityFrameworkCore;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Infrastructure.Repositories;

/// <summary>
/// Repository for AuditLog entities.
/// Note: Audit logs are immutable - only Add and Read operations.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByActionAsync(AuditAction action, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.OccurredAtUtc >= fromUtc && a.OccurredAtUtc <= toUtc)
            .OrderByDescending(a => a.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }
}
