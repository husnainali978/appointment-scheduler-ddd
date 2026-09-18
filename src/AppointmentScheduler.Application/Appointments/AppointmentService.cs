using AppointmentScheduler.Application.Abstractions;
using AppointmentScheduler.Application.Exceptions;
using AppointmentScheduler.Domain.Appointments;
using AppointmentScheduler.Domain.Providers;

namespace AppointmentScheduler.Application.Appointments;

/// <summary>
/// Thin application service: it fetches aggregates, hands work to them, and
/// persists the result. All scheduling/cancellation/reschedule rules live on
/// <see cref="Provider"/> and <see cref="Appointment"/> themselves - this
/// class never inspects a TimeSlot or compares appointments directly.
/// </summary>
public sealed class AppointmentService : IAppointmentService
{
    private readonly IProviderRepository _providerRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public AppointmentService(
        IProviderRepository providerRepository,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _providerRepository = providerRepository;
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<AppointmentDto> BookAsync(BookAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdWithAppointmentsAsync(request.ProviderId, cancellationToken)
            ?? throw NotFoundException.ForEntity(nameof(Provider), request.ProviderId);

        var slot = new TimeSlot(request.Start, request.End);
        var patient = new PatientInfo(request.PatientName, request.PatientEmail);
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        // Provider owns conflict detection against its own booked calendar
        // and, if it's free, creates the Appointment aggregate itself.
        var appointment = provider.ScheduleAppointment(patient, slot, now, request.Notes);

        _appointmentRepository.Add(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AppointmentDto.FromDomain(appointment);
    }

    public async Task<AppointmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity(nameof(Appointment), id);

        return AppointmentDto.FromDomain(appointment);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetByProviderIdAsync(providerId, cancellationToken);
        return appointments.Select(AppointmentDto.FromDomain).ToList();
    }

    public async Task<AppointmentDto> CancelAsync(Guid id, CancelAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity(nameof(Appointment), id);

        appointment.Cancel(request.Reason, _timeProvider.GetUtcNow().UtcDateTime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AppointmentDto.FromDomain(appointment);
    }

    public async Task<AppointmentDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity(nameof(Appointment), id);

        appointment.Complete(_timeProvider.GetUtcNow().UtcDateTime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AppointmentDto.FromDomain(appointment);
    }

    public async Task<AppointmentDto> RescheduleAsync(Guid id, RescheduleAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity(nameof(Appointment), id);

        var provider = await _providerRepository.GetByIdWithAppointmentsAsync(existing.ProviderId, cancellationToken)
            ?? throw NotFoundException.ForEntity(nameof(Provider), existing.ProviderId);

        var newSlot = new TimeSlot(request.Start, request.End);

        // Provider re-checks the new slot against its whole calendar before
        // letting the appointment move.
        provider.RescheduleAppointment(id, newSlot, _timeProvider.GetUtcNow().UtcDateTime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AppointmentDto.FromDomain(existing);
    }
}
