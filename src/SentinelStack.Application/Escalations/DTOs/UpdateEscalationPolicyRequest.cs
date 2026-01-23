using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Escalations.DTOs;

/// <summary>
/// Request model for updating an escalation policy.
/// </summary>
public class UpdateEscalationPolicyRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public Guid? ServiceId { get; init; }

    public bool? IsActive { get; init; }

    public bool? IsDefault { get; init; }

    /// <summary>
    /// If provided, replaces all existing steps with these new steps.
    /// </summary>
    public List<CreateEscalationStepRequest>? Steps { get; init; }
}
