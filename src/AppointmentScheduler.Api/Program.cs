using AppointmentScheduler.Api.Middleware;
using AppointmentScheduler.Application.Appointments;
using AppointmentScheduler.Application.Providers;
using AppointmentScheduler.Infrastructure;
using AppointmentScheduler.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// TimeProvider.System is injected wherever the application layer needs
// "now" (e.g. rejecting appointments booked in the past), so tests can swap
// in a FakeTimeProvider instead of depending on DateTime.UtcNow directly.
builder.Services.AddSingleton(TimeProvider.System);

// Application layer services - thin orchestrators over the domain.
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Everything EF Core/SQLite-related: DbContext, repositories, the domain
// event dispatcher and its handlers.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Apply any pending EF Core migrations on startup, so the SQLite database
// and schema exist with zero extra setup steps for anyone cloning the repo.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program;
