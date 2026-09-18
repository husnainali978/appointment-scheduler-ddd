using AppointmentScheduler.Domain.Appointments.Events;
using AppointmentScheduler.Domain.Common;
using Microsoft.Extensions.Logging;

namespace AppointmentScheduler.Application.EventHandlers;

/// <summary>Reacts to a rescheduled appointment (would notify the patient/provider).</summary>
public sealed class AppointmentRescheduledEventHandler : IDomainEventHandler<AppointmentRescheduledEvent>
{
    private readonly ILogger<AppointmentRescheduledEventHandler> _logger;

    public AppointmentRescheduledEventHandler(ILogger<AppointmentRescheduledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(AppointmentRescheduledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Appointment {AppointmentId} with provider {ProviderId} moved from {PreviousStart:u} to {NewStart:u}. Notification would be sent now.",
            domainEvent.AppointmentId, domainEvent.ProviderId, domainEvent.PreviousStart, domainEvent.NewStart);

        return Task.CompletedTask;
    }
}
