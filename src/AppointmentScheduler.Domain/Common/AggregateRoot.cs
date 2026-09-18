namespace AppointmentScheduler.Domain.Common;

/// <summary>
/// Base type for aggregate roots: entities that are the entry point of a
/// consistency boundary and the only objects the outside world (application
/// layer, repositories) is allowed to load and save directly.
///
/// Aggregate roots accumulate domain events as their invariants change. The
/// events sit here, uncommitted, until infrastructure dispatches them after a
/// successful SaveChanges and clears them.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
