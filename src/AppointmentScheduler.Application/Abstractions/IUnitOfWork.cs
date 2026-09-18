namespace AppointmentScheduler.Application.Abstractions;

/// <summary>
/// Commits the current unit of work. Implemented in Infrastructure by the
/// EF Core DbContext, whose overridden SaveChangesAsync is also where
/// collected domain events get dispatched once the transaction succeeds.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
