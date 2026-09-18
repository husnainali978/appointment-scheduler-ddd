using AppointmentScheduler.Application.Abstractions;
using AppointmentScheduler.Domain.Providers;
using Microsoft.EntityFrameworkCore;

namespace AppointmentScheduler.Infrastructure.Persistence.Repositories;

public sealed class ProviderRepository : IProviderRepository
{
    private readonly SchedulingDbContext _dbContext;

    public ProviderRepository(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Providers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Provider?> GetByIdWithAppointmentsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Providers
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Providers
            .Include(p => p.Appointments)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public void Add(Provider provider) => _dbContext.Providers.Add(provider);
}
