namespace SentinelStack.Domain.Common;

/// <summary>
/// Base entity with audit trail properties for HIPAA compliance.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// UTC timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; protected set; }

    /// <summary>
    /// ID of the user who created this entity.
    /// </summary>
    public Guid? CreatedBy { get; protected set; }

    /// <summary>
    /// UTC timestamp when the entity was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; protected set; }

    /// <summary>
    /// ID of the user who last modified this entity.
    /// </summary>
    public Guid? UpdatedBy { get; protected set; }

    protected AuditableEntity()
    {
        CreatedAtUtc = DateTime.UtcNow;
    }

    protected AuditableEntity(Guid id) : base(id)
    {
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the creation audit fields.
    /// </summary>
    public void SetCreatedBy(Guid userId)
    {
        CreatedBy = userId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the update audit fields.
    /// </summary>
    public void SetUpdatedBy(Guid userId)
    {
        UpdatedBy = userId;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
