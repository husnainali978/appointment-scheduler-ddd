using AppointmentScheduler.Application.Abstractions;
using AppointmentScheduler.Domain.Appointments;
using AppointmentScheduler.Domain.Common;
using AppointmentScheduler.Domain.Providers;
using Microsoft.EntityFrameworkCore;

namespace AppointmentScheduler.Infrastructure.Persistence;

/// <summary>
/// The one EF Core-aware type that knows about the domain model's shape.
/// Entity configuration is split out into <see cref="IEntityTypeConfiguration{TEntity}"/>
/// classes so this class stays a thin composition root, and so the Domain
/// project itself never needs to reference EF Core.
/// </summary>
public sealed class SchedulingDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
        : base(options)
    {
    }

    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options, IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulingDbContext).Assembly);
    }

    /// <summary>
    /// Persists changes, then - only once the transaction has actually
    /// succeeded - dispatches whatever domain events were raised by the
    /// aggregates involved. This guarantees handlers never see an event for
    /// a change that didn't make it to the database.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_domainEventDispatcher is not null && aggregatesWithEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchAndClearEventsAsync(aggregatesWithEvents, cancellationToken);
        }

        return result;
    }
}
