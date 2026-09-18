using AppointmentScheduler.Domain.Appointments.Events;
using AppointmentScheduler.Domain.Common;
using Microsoft.Extensions.Logging;

namespace AppointmentScheduler.Application.EventHandlers;

/// <summary>Reacts to a cancelled appointment (would notify the patient/provider).</summary>
public sealed class AppointmentCancelledEventHandler : IDomainEventHandler<AppointmentCancelledEvent>
{
    private readonly ILogger<AppointmentCancelledEventHandler> _logger;

    public AppointmentCancelledEventHandler(ILogger<AppointmentCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(AppointmentCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Appointment {AppointmentId} with provider {ProviderId} was cancelled. Reason: {Reason}. Notification would be sent now.",
            domainEvent.AppointmentId, domainEvent.ProviderId, domainEvent.Reason);

        return Task.CompletedTask;
    }
}
