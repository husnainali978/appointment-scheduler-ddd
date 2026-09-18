using AppointmentScheduler.Domain.Exceptions;

namespace AppointmentScheduler.Domain.Appointments;

/// <summary>
/// Value object capturing who an appointment is for. Kept intentionally small
/// - this bounded context cares about scheduling, not about being the
/// system of record for patient demographics.
/// </summary>
public sealed record PatientInfo
{
    public string Name { get; }
    public string Email { get; }

    public PatientInfo(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Patient name is required.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid patient email is required.");

        Name = name.Trim();
        Email = email.Trim();
    }
}
