using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Escalations.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Escalations.Commands;

/// <summary>
/// Command to create a new escalation policy.
/// </summary>
public class CreateEscalationPolicyCommand
{
    private readonly IEscalationPolicyRepository _policyRepository;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEscalationPolicyCommand(
        IEscalationPolicyRepository policyRepository,
        ICurrentTenantService tenantService,
        ICurrentUserService userService,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _tenantService = tenantService;
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EscalationPolicyDto> ExecuteAsync(CreateEscalationPolicyRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId
            ?? throw new InvalidOperationException("Tenant context is required");

        var policy = new EscalationPolicy(
            tenantId,
            request.Name,
            request.Description,
            request.ServiceId);

        if (request.IsDefault)
        {
            // Clear default from other policies first
            var existingDefault = await _policyRepository.GetDefaultPolicyAsync(cancellationToken);
            if (existingDefault != null)
            {
                existingDefault.RemoveDefault();
            }
            policy.SetAsDefault();
        }

        // Add steps
        foreach (var stepRequest in request.Steps.OrderBy(s => s.StepOrder))
        {
            var step = new EscalationStep(
                policy.Id,
                stepRequest.StepOrder,
                stepRequest.DelayMinutes,
                stepRequest.NotificationType,
                stepRequest.NotifyUserId,
                stepRequest.NotifyTeam,
                stepRequest.NotifyEmails,
                stepRequest.MessageTemplate,
                stepRequest.RepeatIfUnacknowledged,
                stepRequest.RepeatIntervalMinutes,
                stepRequest.MaxRepeats);

            policy.AddStep(step);
        }

        var userId = _userService.UserId;
        if (userId.HasValue)
        {
            policy.SetCreatedBy(userId.Value);
        }

        await _policyRepository.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(policy);
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
