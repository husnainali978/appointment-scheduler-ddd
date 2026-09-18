using AppointmentScheduler.Domain.Appointments;
using AppointmentScheduler.Domain.Common;
using AppointmentScheduler.Domain.Exceptions;

namespace AppointmentScheduler.Domain.Providers;

/// <summary>
/// Aggregate root for a service/appointment provider (e.g. a doctor,
/// therapist, or stylist). Owns the rule that really matters in a scheduling
/// system: a provider cannot be double-booked. Conflict detection lives here,
/// against the provider's own booked appointments, rather than in an
/// application service running ad-hoc LINQ over a flat appointment table.
/// </summary>
public class Provider : AggregateRoot
{
    private readonly List<Appointment> _appointments = [];

    public string Name { get; private set; } = null!;
    public string Specialty { get; private set; } = null!;

    /// <summary>
    /// The appointments this provider knows about. Populated by the
    /// repository (eager-loaded) whenever conflict-checking is required;
    /// exposed read-only so callers can't mutate the calendar directly.
    /// </summary>
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    /// <summary>Reserved for EF Core materialization.</summary>
    private Provider()
    {
    }

    public Provider(string name, string specialty)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Provider name is required.");

        if (string.IsNullOrWhiteSpace(specialty))
            throw new DomainException("Provider specialty is required.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Specialty = specialty.Trim();
    }

    /// <summary>
    /// True when <paramref name="slot"/> does not overlap with any of this
    /// provider's currently scheduled appointments. Cancelled appointments no
    /// longer hold their slot, so they're excluded.
    /// </summary>
    public bool CanSchedule(TimeSlot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);

        return _appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled)
            .All(a => !a.Slot.Overlaps(slot));
    }

    /// <summary>
    /// Books a new appointment with this provider, rejecting it outright if
    /// it would conflict with an existing one. This is the single entry point
    /// application code should use to schedule an appointment - it keeps
    /// conflict-checking and appointment creation atomic from the domain's
    /// point of view.
    /// </summary>
    public Appointment ScheduleAppointment(PatientInfo patient, TimeSlot slot, DateTime nowUtc, string? notes = null)
    {
        if (!CanSchedule(slot))
        {
            throw new SchedulingConflictException(
                $"{Name} already has an appointment that overlaps with {slot}.");
        }

        var appointment = Appointment.Schedule(Id, patient, slot, nowUtc, notes);
        _appointments.Add(appointment);
        return appointment;
    }

    /// <summary>
    /// Reschedules one of this provider's own appointments, re-running
    /// conflict detection against the new slot before committing to it.
    /// </summary>
    public void RescheduleAppointment(Guid appointmentId, TimeSlot newSlot, DateTime nowUtc)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId)
            ?? throw new DomainException($"Provider {Name} has no appointment with id '{appointmentId}'.");

        var wouldConflict = _appointments
            .Where(a => a.Id != appointmentId && a.Status == AppointmentStatus.Scheduled)
            .Any(a => a.Slot.Overlaps(newSlot));

        if (wouldConflict)
        {
            throw new SchedulingConflictException(
                $"{Name} already has an appointment that overlaps with {newSlot}.");
        }

        appointment.Reschedule(newSlot, nowUtc);
    }
}
