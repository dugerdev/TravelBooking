using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace TravelBooking.Api.Controllers.Admin;

[Route("api/admin/reservations")]
[Authorize(Roles = "Admin")]
public sealed class ReservationsAdminController : BaseController
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsAdminController> _logger;
    private readonly IMapper _mapper;

    public ReservationsAdminController(IReservationService reservationService, ILogger<ReservationsAdminController> logger, IMapper mapper)
    {
        _reservationService = reservationService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<DataResult<IEnumerable<ReservationDto>>>> GetAll(
        [FromQuery] ReservationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.GetAllForAdminAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Rezervasyon verisi bulunamadı.");

        var reservations = result.Data;
        
        // Filter by status if provided
        if (status.HasValue)
        {
            reservations = reservations.Where(r => r.Status == status.Value);
        }

        var reservationDtos = _mapper.Map<IEnumerable<ReservationDto>>(reservations);

        return Ok(new SuccessDataResult<IEnumerable<ReservationDto>>(reservationDtos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataResult<ReservationDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reservationService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var reservationDto = _mapper.Map<ReservationDto>(result.Data);
        reservationDto.Tickets ??= [];
        reservationDto.Passengers ??= [];
        reservationDto.Payments ??= [];

        return Ok(new SuccessDataResult<ReservationDto>(reservationDto));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<Result>> CancelReservation(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reservationService.CancelReservationAsync(id, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        _logger.LogInformation("Reservation {ReservationId} cancelled by admin", id);
        return Ok(new SuccessResult(result.Message ?? "Rezervasyon iptal edildi."));
    }
}
