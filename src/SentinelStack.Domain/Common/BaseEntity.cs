namespace SentinelStack.Domain.Common;

/// <summary>
/// Base entity with common properties for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Domain events raised by this entity.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event.
    /// </summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }
}

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
