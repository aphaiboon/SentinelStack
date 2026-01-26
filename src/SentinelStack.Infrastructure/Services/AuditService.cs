using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Service for creating audit log entries with automatic context capture.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditService(
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        ICurrentTenantService currentTenantService,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _currentTenantService = currentTenantService;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task LogCreateAsync<TEntity>(Guid entityId, TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;

        var auditLog = new AuditLog(
            tenantId,
            AuditAction.Created,
            typeof(TEntity).Name,
            entityId,
            userId,
            userEmail,
            GetClientIpAddress(),
            GetUserAgent(),
            null,
            SerializeEntity(entity),
            null,
            GetCorrelationId());

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogUpdateAsync<TEntity>(Guid entityId, TEntity? oldEntity, TEntity newEntity, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;

        var auditLog = new AuditLog(
            tenantId,
            AuditAction.Updated,
            typeof(TEntity).Name,
            entityId,
            userId,
            userEmail,
            GetClientIpAddress(),
            GetUserAgent(),
            oldEntity != null ? SerializeEntity(oldEntity) : null,
            SerializeEntity(newEntity),
            null,
            GetCorrelationId());

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogDeleteAsync<TEntity>(Guid entityId, TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;

        var auditLog = new AuditLog(
            tenantId,
            AuditAction.Deleted,
            typeof(TEntity).Name,
            entityId,
            userId,
            userEmail,
            GetClientIpAddress(),
            GetUserAgent(),
            SerializeEntity(entity),
            null,
            null,
            GetCorrelationId());

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogAccessAsync<TEntity>(Guid entityId, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;

        var auditLog = new AuditLog(
            tenantId,
            AuditAction.Accessed,
            typeof(TEntity).Name,
            entityId,
            userId,
            userEmail,
            GetClientIpAddress(),
            GetUserAgent());

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogActionAsync(
        AuditAction action,
        string entityType,
        Guid? entityId,
        string? notes = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;

        var auditLog = new AuditLog(
            tenantId,
            action,
            entityType,
            entityId,
            userId,
            userEmail,
            GetClientIpAddress(),
            GetUserAgent(),
            oldValues,
            newValues,
            notes,
            GetCorrelationId());

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogLoginAsync(Guid userId, string email, bool success, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;

        var auditLog = AuditLog.ForLogin(
            tenantId,
            userId,
            email,
            GetClientIpAddress(),
            GetUserAgent(),
            success);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogLogoutAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;

        var auditLog = new AuditLog(
            tenantId,
            AuditAction.Logout,
            nameof(User),
            userId,
            userId,
            email,
            GetClientIpAddress(),
            GetUserAgent());

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private string? GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return null;
        }

        // Check for forwarded headers first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
    }

    private string? GetCorrelationId()
    {
        return _httpContextAccessor.HttpContext?.TraceIdentifier;
    }

    private static string? SerializeEntity<T>(T entity)
    {
        try
        {
            return JsonSerializer.Serialize(entity, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
