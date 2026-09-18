namespace AppointmentScheduler.Application.Providers;

public interface IProviderService
{
    Task<ProviderDto> CreateAsync(CreateProviderRequest request, CancellationToken cancellationToken = default);

    Task<ProviderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
