namespace AppointmentScheduler.Domain.Common;

/// <summary>
/// Marker for something that happened in the domain that other parts of the
/// system (or other bounded contexts) might care about. Raised by aggregates,
/// collected after SaveChanges, and handed to in-process handlers.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
