using AppointmentScheduler.Application.Appointments;
using AppointmentScheduler.Application.Providers;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentScheduler.Api.Controllers;

[ApiController]
[Route("api/providers")]
public sealed class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;
    private readonly IAppointmentService _appointmentService;

    public ProvidersController(IProviderService providerService, IAppointmentService appointmentService)
    {
        _providerService = providerService;
        _appointmentService = appointmentService;
    }

    /// <summary>Registers a new provider (e.g. a doctor or therapist).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProviderDto>> Create([FromBody] CreateProviderRequest request, CancellationToken cancellationToken)
    {
        var provider = await _providerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = provider.Id }, provider);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProviderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProviderDto>>> GetAll(CancellationToken cancellationToken)
    {
        var providers = await _providerService.GetAllAsync(cancellationToken);
        return Ok(providers);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var provider = await _providerService.GetByIdAsync(id, cancellationToken);
        return Ok(provider);
    }

    /// <summary>Lists every appointment (scheduled, completed, or cancelled) booked with this provider.</summary>
    [HttpGet("{id:guid}/appointments")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAppointments(Guid id, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetByProviderIdAsync(id, cancellationToken);
        return Ok(appointments);
    }
}
