namespace AppointmentScheduler.Domain.Exceptions;

/// <summary>
/// Raised when an operation would violate a domain invariant (e.g. booking an
/// appointment in the past, cancelling an already-cancelled appointment).
/// The API layer maps this to HTTP 400.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
