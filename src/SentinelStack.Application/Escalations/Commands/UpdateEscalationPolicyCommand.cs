using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Escalations.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Escalations.Commands;

/// <summary>
/// Command to update an existing escalation policy.
/// </summary>
public class UpdateEscalationPolicyCommand
{
    private readonly IEscalationPolicyRepository _policyRepository;
    private readonly ICurrentUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEscalationPolicyCommand(
        IEscalationPolicyRepository policyRepository,
        ICurrentUserService userService,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EscalationPolicyDto?> ExecuteAsync(Guid id, UpdateEscalationPolicyRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetWithStepsAsync(id, cancellationToken);
        if (policy == null)
        {
            return null;
        }

        policy.Update(request.Name, request.Description);

        if (request.ServiceId.HasValue)
        {
            policy.AssociateWithService(request.ServiceId.Value);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                policy.Activate();
            }
            else
            {
                policy.Deactivate();
            }
        }

        if (request.IsDefault.HasValue)
        {
            if (request.IsDefault.Value)
            {
                // Clear default from other policies first
                var existingDefault = await _policyRepository.GetDefaultPolicyAsync(cancellationToken);
                if (existingDefault != null && existingDefault.Id != policy.Id)
                {
                    existingDefault.RemoveDefault();
                }
                policy.SetAsDefault();
            }
            else
            {
                policy.RemoveDefault();
            }
        }

        // Replace steps if provided
        if (request.Steps != null)
        {
            // Remove existing steps
            foreach (var existingStep in policy.Steps.ToList())
            {
                policy.RemoveStep(existingStep);
            }

            // Add new steps
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
        }

        var userId = _userService.UserId;
        if (userId.HasValue)
        {
            policy.SetUpdatedBy(userId.Value);
        }

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
