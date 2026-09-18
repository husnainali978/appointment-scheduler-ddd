using AppointmentScheduler.Application.Abstractions;
using AppointmentScheduler.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace AppointmentScheduler.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly SchedulingDbContext _dbContext;

    public AppointmentRepository(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default) =>
        await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ProviderId == providerId)
            .OrderBy(a => a.Slot.Start)
            .ToListAsync(cancellationToken);

    public void Add(Appointment appointment) => _dbContext.Appointments.Add(appointment);
}
