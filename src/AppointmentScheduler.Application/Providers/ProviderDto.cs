using AppointmentScheduler.Domain.Providers;

namespace AppointmentScheduler.Application.Providers;

public sealed record ProviderDto(Guid Id, string Name, string Specialty, int BookedAppointmentCount)
{
    public static ProviderDto FromDomain(Provider provider) => new(
        provider.Id,
        provider.Name,
        provider.Specialty,
        provider.Appointments.Count);
}

public sealed record CreateProviderRequest(string Name, string Specialty);
