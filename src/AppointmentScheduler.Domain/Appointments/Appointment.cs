using AppointmentScheduler.Domain.Appointments.Events;
using AppointmentScheduler.Domain.Common;
using AppointmentScheduler.Domain.Exceptions;

namespace AppointmentScheduler.Domain.Appointments;

/// <summary>
/// Aggregate root for a single booked appointment. The only way to create one
/// is through <see cref="Schedule"/>, which enforces every invariant an
/// appointment must satisfy at creation time. State transitions (cancel,
/// complete, reschedule) are exposed as intention-revealing methods rather
/// than property setters, so the aggregate always stays valid.
/// </summary>
public class Appointment : AggregateRoot
{
    public Guid ProviderId { get; private set; }
    public PatientInfo Patient { get; private set; } = null!;
    public TimeSlot Slot { get; private set; } = null!;
    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    /// <summary>Reserved for EF Core materialization.</summary>
    private Appointment()
    {
    }

    private Appointment(Guid providerId, PatientInfo patient, TimeSlot slot, DateTime nowUtc, string? notes)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        Patient = patient;
        Slot = slot;
        Notes = notes;
        Status = AppointmentStatus.Scheduled;
        CreatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Factory method that is the only entry point for creating an
    /// appointment. Enforces that the appointment belongs to a real provider,
    /// that its slot is well-formed (guaranteed by <see cref="TimeSlot"/>
    /// itself) and that it isn't being booked in the past.
    /// </summary>
    public static Appointment Schedule(
        Guid providerId,
        PatientInfo patient,
        TimeSlot slot,
        DateTime nowUtc,
        string? notes = null)
    {
        if (providerId == Guid.Empty)
            throw new DomainException("An appointment must be associated with a provider.");

        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(slot);

        if (slot.IsInPast(nowUtc))
            throw new DomainException("Cannot book an appointment in the past.");

        var appointment = new Appointment(providerId, patient, slot, nowUtc, notes);
        appointment.RaiseDomainEvent(new AppointmentBookedEvent(
            appointment.Id, providerId, slot.Start, slot.End, nowUtc));

        return appointment;
    }

    public void Cancel(string reason, DateTime nowUtc)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new DomainException("This appointment has already been cancelled.");

        if (Status == AppointmentStatus.Completed)
            throw new DomainException("A completed appointment cannot be cancelled.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A cancellation reason is required.");

        Status = AppointmentStatus.Cancelled;
        CancelledAtUtc = nowUtc;
        CancellationReason = reason;

        RaiseDomainEvent(new AppointmentCancelledEvent(Id, ProviderId, reason, nowUtc));
    }

    public void Complete(DateTime nowUtc)
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new DomainException("Only a scheduled appointment can be marked as completed.");

        if (Slot.Start > nowUtc)
            throw new DomainException("An appointment cannot be completed before its scheduled start time.");

        Status = AppointmentStatus.Completed;
    }

    /// <summary>
    /// Moves this appointment to a new slot. Conflict-checking against the
    /// provider's other appointments is the provider's responsibility
    /// (<see cref="Provider.CanSchedule"/>), since only the provider knows
    /// about its full booked calendar; this method only enforces the
    /// invariants that belong to the appointment itself.
    /// </summary>
    public void Reschedule(TimeSlot newSlot, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(newSlot);

        if (Status != AppointmentStatus.Scheduled)
            throw new DomainException("Only a scheduled appointment can be rescheduled.");

        if (newSlot.IsInPast(nowUtc))
            throw new DomainException("Cannot reschedule an appointment into the past.");

        var previousSlot = Slot;
        Slot = newSlot;

        RaiseDomainEvent(new AppointmentRescheduledEvent(
            Id, ProviderId, previousSlot.Start, previousSlot.End, newSlot.Start, newSlot.End, nowUtc));
    }
}
