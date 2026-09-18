using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppointmentScheduler.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add/update` construct a DbContext at design
/// time, without needing to spin up the whole API host or resolve a domain
/// event dispatcher from DI. Not used at runtime.
/// </summary>
public sealed class SchedulingDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
{
    public SchedulingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SchedulingDbContext>();
        optionsBuilder.UseSqlite("Data Source=appointments.db");

        return new SchedulingDbContext(optionsBuilder.Options);
    }
}
