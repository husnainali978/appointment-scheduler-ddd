using AppointmentScheduler.Domain.Common;

namespace AppointmentScheduler.Domain.Appointments.Events;

/// <summary>Raised when a previously scheduled appointment is cancelled.</summary>
public sealed record AppointmentCancelledEvent(
    Guid AppointmentId,
    Guid ProviderId,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
