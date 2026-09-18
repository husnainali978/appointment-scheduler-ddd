using AppointmentScheduler.Application.Abstractions;
using AppointmentScheduler.Application.EventHandlers;
using AppointmentScheduler.Domain.Appointments.Events;
using AppointmentScheduler.Domain.Common;
using AppointmentScheduler.Infrastructure.Events;
using AppointmentScheduler.Infrastructure.Persistence;
using AppointmentScheduler.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentScheduler.Infrastructure;

/// <summary>
/// Composition root for everything Infrastructure owns: the DbContext,
/// repositories, the domain event dispatcher, and the handlers that react to
/// domain events. Kept as a single extension method so the API project's
/// Program.cs doesn't need to know about EF Core or SQLite at all.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=appointments.db";

        services.AddDbContext<SchedulingDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SchedulingDbContext>());

        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<AppointmentBookedEvent>, AppointmentBookedEventHandler>();
        services.AddScoped<IDomainEventHandler<AppointmentCancelledEvent>, AppointmentCancelledEventHandler>();
        services.AddScoped<IDomainEventHandler<AppointmentRescheduledEvent>, AppointmentRescheduledEventHandler>();

        return services;
    }
}
