namespace AppointmentScheduler.Application.Exceptions;

/// <summary>
/// Raised when a requested entity doesn't exist. This is an application-level
/// concern (it's about a lookup failing), not a domain rule violation, so it
/// deliberately doesn't extend DomainException. The API layer maps it to
/// HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException ForEntity(string entityName, Guid id) =>
        new($"{entityName} '{id}' was not found.");
}
