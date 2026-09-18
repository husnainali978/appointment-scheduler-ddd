using AppointmentScheduler.Domain.Providers;

namespace AppointmentScheduler.Application.Abstractions;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a provider together with its booked appointments, which is what
    /// callers need whenever they're about to call
    /// <see cref="Provider.CanSchedule"/> or <see cref="Provider.ScheduleAppointment"/>.
    /// </summary>
    Task<Provider?> GetByIdWithAppointmentsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(Provider provider);
}
