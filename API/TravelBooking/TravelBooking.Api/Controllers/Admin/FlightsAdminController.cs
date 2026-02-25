using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace TravelBooking.Api.Controllers.Admin;

/// <summary>
/// Admin uçuş yönetimi için API endpoint'leri.
/// Tüm endpoint'ler Admin rolü gerektirir.
/// </summary>
[Route("api/admin/flights")]
[Authorize(Roles = "Admin")]
public sealed class FlightsAdminController : BaseController
{
    private readonly IFlightService _flightService;
    private readonly ILogger<FlightsAdminController> _logger;
    private readonly IMapper _mapper;

    public FlightsAdminController(IFlightService flightService, ILogger<FlightsAdminController> logger, IMapper mapper)
    {
        _flightService = flightService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<DataResult<PagedResult<FlightDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new TravelBooking.Application.Common.PagedRequest
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize)
        };

        var result = await _flightService.GetAllPagedAsync(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Uçuş verisi bulunamadı.");

        var flightDtos = _mapper.Map<IEnumerable<FlightDto>>(result.Data.Items);
        var pagedResult = new TravelBooking.Application.Common.PagedResult<FlightDto>(
            flightDtos,
            result.Data.TotalCount,
            result.Data.PageNumber,
            result.Data.PageSize);

        return Ok(new SuccessDataResult<TravelBooking.Application.Common.PagedResult<FlightDto>>(pagedResult));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataResult<FlightDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _flightService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var flightDto = _mapper.Map<FlightDto>(result.Data);

        return Ok(new SuccessDataResult<FlightDto>(flightDto));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] CreateFlightDto dto, CancellationToken cancellationToken)
    {
        var result = await _flightService.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var flight = result.Data;
        
        // Update flight schedule if changed
        if (dto.ScheduledDeparture != flight.ScheduledDeparture || dto.ScheduledArrival != flight.ScheduledArrival)
        {
            flight.UpdateSchedule(dto.ScheduledDeparture, dto.ScheduledArrival);
        }

        // Note: Other fields are private setters, so they can't be updated directly
        // You may need to add update methods to Flight entity if needed
        
        var updateResult = await _flightService.UpdateAsync(flight, cancellationToken);
        if (!updateResult.Success)
            return BadRequest(updateResult);

        return Ok(updateResult);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _flightService.DeleteAsync(id, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        _logger.LogInformation("Flight {FlightId} deleted by admin", id);
        return Ok(result);
    }
}
