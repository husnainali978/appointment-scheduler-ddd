using AppointmentScheduler.Application.Appointments;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentScheduler.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Books an appointment with a provider. Rejected with 409 Conflict if
    /// the requested time slot overlaps with one the provider already has.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDto>> Book([FromBody] BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.BookAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);
        return Ok(appointment);
    }

    /// <summary>Cancels a scheduled appointment, freeing up its time slot.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> Cancel(Guid id, [FromBody] CancelAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.CancelAsync(id, request, cancellationToken);
        return Ok(appointment);
    }

    /// <summary>Marks an appointment as completed once it has taken place.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.CompleteAsync(id, cancellationToken);
        return Ok(appointment);
    }

    /// <summary>
    /// Moves a scheduled appointment to a new time slot. Rejected with
    /// 409 Conflict if the new slot overlaps with another of the provider's
    /// appointments.
    /// </summary>
    [HttpPut("{id:guid}/reschedule")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDto>> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.RescheduleAsync(id, request, cancellationToken);
        return Ok(appointment);
    }
}
