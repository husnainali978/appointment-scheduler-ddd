namespace AppointmentScheduler.Domain.Common;

/// <summary>
/// Handles one kind of domain event. Implemented in the Application layer
/// (or Infrastructure, for cross-cutting concerns) and resolved by the
/// dispatcher through DI. No message bus involved - this is purely in-process.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dispatches the domain events raised by a batch of aggregates to whatever
/// handlers are registered for each event type, then clears them so they are
/// never re-dispatched. Infrastructure invokes this right after a successful
/// SaveChanges.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAndClearEventsAsync(IEnumerable<AggregateRoot> aggregatesWithEvents, CancellationToken cancellationToken = default);
}
