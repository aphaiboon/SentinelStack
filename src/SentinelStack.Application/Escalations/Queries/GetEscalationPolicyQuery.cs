using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Escalations.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Escalations.Queries;

/// <summary>
/// Query to get a single escalation policy.
/// </summary>
public class GetEscalationPolicyQuery
{
    private readonly IEscalationPolicyRepository _policyRepository;

    public GetEscalationPolicyQuery(IEscalationPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<EscalationPolicyDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetWithStepsAsync(id, cancellationToken);
        return policy == null ? null : MapToDto(policy);
    }

    public async Task<EscalationPolicyDto?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetDefaultPolicyAsync(cancellationToken);
        return policy == null ? null : MapToDto(policy);
    }

    public async Task<EscalationPolicyDto?> GetByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetByServiceAsync(serviceId, cancellationToken);
        return policy == null ? null : MapToDto(policy);
    }

    private static EscalationPolicyDto MapToDto(EscalationPolicy policy) => new()
    {
        Id = policy.Id,
        TenantId = policy.TenantId,
        Name = policy.Name,
        Description = policy.Description,
        IsActive = policy.IsActive,
        IsDefault = policy.IsDefault,
        ServiceId = policy.ServiceId,
        Steps = policy.Steps.Select(s => new EscalationStepDto
        {
            Id = s.Id,
            StepOrder = s.StepOrder,
            DelayMinutes = s.DelayMinutes,
            NotificationType = s.NotificationType,
            NotifyUserId = s.NotifyUserId,
            NotifyTeam = s.NotifyTeam,
            NotifyEmails = s.NotifyEmails,
            MessageTemplate = s.MessageTemplate,
            RepeatIfUnacknowledged = s.RepeatIfUnacknowledged,
            RepeatIntervalMinutes = s.RepeatIntervalMinutes,
            MaxRepeats = s.MaxRepeats
        }).ToList(),
        CreatedAtUtc = policy.CreatedAtUtc,
        CreatedBy = policy.CreatedBy,
        UpdatedAtUtc = policy.UpdatedAtUtc,
        UpdatedBy = policy.UpdatedBy
    };
}
