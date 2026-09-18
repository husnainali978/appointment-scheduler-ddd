namespace AppointmentScheduler.Domain.Exceptions;

/// <summary>
/// Raised when a provider is asked to schedule an appointment whose time slot
/// overlaps with one they already have booked. The API layer maps this to
/// HTTP 409 Conflict.
/// </summary>
public sealed class SchedulingConflictException : DomainException
{
    public SchedulingConflictException(string message) : base(message)
    {
    }
}
