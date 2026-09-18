using AppointmentScheduler.Application.Abstractions;
using AppointmentScheduler.Application.Exceptions;
using AppointmentScheduler.Domain.Providers;

namespace AppointmentScheduler.Application.Providers;

/// <summary>
/// Thin application service: it orchestrates repositories and the domain,
/// it doesn't contain business rules itself. Provider construction and
/// validation both happen inside the <see cref="Provider"/> aggregate.
/// </summary>
public sealed class ProviderService : IProviderService
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProviderService(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProviderDto> CreateAsync(CreateProviderRequest request, CancellationToken cancellationToken = default)
    {
        var provider = new Provider(request.Name, request.Specialty);

        _providerRepository.Add(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProviderDto.FromDomain(provider);
    }

    public async Task<ProviderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdWithAppointmentsAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity(nameof(Provider), id);

        return ProviderDto.FromDomain(provider);
    }

    public async Task<IReadOnlyList<ProviderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _providerRepository.GetAllAsync(cancellationToken);
        return providers.Select(ProviderDto.FromDomain).ToList();
    }
}
