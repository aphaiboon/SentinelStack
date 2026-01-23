using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Escalations.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Escalations.Queries;

/// <summary>
/// Request model for filtering escalation policies.
/// </summary>
public class GetEscalationPoliciesRequest
{
    public bool? IsActive { get; init; }
    public Guid? ServiceId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Response model for paginated escalation policies.
/// </summary>
public class GetEscalationPoliciesResponse
{
    public required IReadOnlyList<EscalationPolicyDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Query to get escalation policies with filtering and pagination.
/// </summary>
public class GetEscalationPoliciesQuery
{
    private readonly IEscalationPolicyRepository _policyRepository;

    public GetEscalationPoliciesQuery(IEscalationPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<GetEscalationPoliciesResponse> ExecuteAsync(GetEscalationPoliciesRequest request, CancellationToken cancellationToken = default)
    {
        var policies = request.IsActive == true
            ? await _policyRepository.GetActivePoliciesAsync(cancellationToken)
            : await _policyRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filtered = policies.AsEnumerable();

        if (request.IsActive.HasValue && request.IsActive == false)
        {
            filtered = filtered.Where(p => !p.IsActive);
        }

        if (request.ServiceId.HasValue)
        {
            filtered = filtered.Where(p => p.ServiceId == request.ServiceId.Value);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var paged = filteredList
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        return new GetEscalationPoliciesResponse
        {
            Items = paged,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
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
