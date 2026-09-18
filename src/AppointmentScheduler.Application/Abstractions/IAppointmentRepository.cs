using AppointmentScheduler.Domain.Appointments;

namespace AppointmentScheduler.Application.Abstractions;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    void Add(Appointment appointment);
}
