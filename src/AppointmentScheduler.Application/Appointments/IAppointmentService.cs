namespace AppointmentScheduler.Application.Appointments;

public interface IAppointmentService
{
    Task<AppointmentDto> BookAsync(BookAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<AppointmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    Task<AppointmentDto> CancelAsync(Guid id, CancelAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<AppointmentDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AppointmentDto> RescheduleAsync(Guid id, RescheduleAppointmentRequest request, CancellationToken cancellationToken = default);
}
