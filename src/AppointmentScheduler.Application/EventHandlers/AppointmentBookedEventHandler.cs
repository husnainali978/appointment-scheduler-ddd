using AppointmentScheduler.Domain.Appointments.Events;
using AppointmentScheduler.Domain.Common;
using Microsoft.Extensions.Logging;

namespace AppointmentScheduler.Application.EventHandlers;

/// <summary>
/// Reacts to a booked appointment. In a real system this would enqueue a
/// confirmation email/SMS; here it just logs, to keep the sample
/// dependency-free while still proving the dispatch pipeline works end to end.
/// </summary>
public sealed class AppointmentBookedEventHandler : IDomainEventHandler<AppointmentBookedEvent>
{
    private readonly ILogger<AppointmentBookedEventHandler> _logger;

    public AppointmentBookedEventHandler(ILogger<AppointmentBookedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(AppointmentBookedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Appointment {AppointmentId} booked with provider {ProviderId} for {Start:u} - {End:u}. Confirmation would be sent now.",
            domainEvent.AppointmentId, domainEvent.ProviderId, domainEvent.Start, domainEvent.End);

        return Task.CompletedTask;
    }
}
