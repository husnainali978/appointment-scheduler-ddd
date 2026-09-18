using AppointmentScheduler.Domain.Appointments;

namespace AppointmentScheduler.Application.Appointments;

public sealed record AppointmentDto(
    Guid Id,
    Guid ProviderId,
    string PatientName,
    string PatientEmail,
    DateTime Start,
    DateTime End,
    string Status,
    string? Notes,
    string? CancellationReason)
{
    public static AppointmentDto FromDomain(Appointment appointment) => new(
        appointment.Id,
        appointment.ProviderId,
        appointment.Patient.Name,
        appointment.Patient.Email,
        appointment.Slot.Start,
        appointment.Slot.End,
        appointment.Status.ToString(),
        appointment.Notes,
        appointment.CancellationReason);
}

public sealed record BookAppointmentRequest(
    Guid ProviderId,
    string PatientName,
    string PatientEmail,
    DateTime Start,
    DateTime End,
    string? Notes);

public sealed record RescheduleAppointmentRequest(DateTime Start, DateTime End);

public sealed record CancelAppointmentRequest(string Reason);
