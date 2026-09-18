using AppointmentScheduler.Domain.Common;

namespace AppointmentScheduler.Domain.Appointments.Events;

/// <summary>Raised when a scheduled appointment's time slot is changed.</summary>
public sealed record AppointmentRescheduledEvent(
    Guid AppointmentId,
    Guid ProviderId,
    DateTime PreviousStart,
    DateTime PreviousEnd,
    DateTime NewStart,
    DateTime NewEnd,
    DateTime OccurredOnUtc) : IDomainEvent;
