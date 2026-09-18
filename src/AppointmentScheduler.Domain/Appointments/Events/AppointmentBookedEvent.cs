using AppointmentScheduler.Domain.Common;

namespace AppointmentScheduler.Domain.Appointments.Events;

/// <summary>Raised when a new appointment is successfully scheduled.</summary>
public sealed record AppointmentBookedEvent(
    Guid AppointmentId,
    Guid ProviderId,
    DateTime Start,
    DateTime End,
    DateTime OccurredOnUtc) : IDomainEvent;
