using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/flights/sync")]
[Authorize(Roles = "Admin")]
public sealed class FlightSyncController : ControllerBase
{
    private readonly IFlightDataSyncService _syncService;
    private readonly ILogger<FlightSyncController> _logger;

    public FlightSyncController(IFlightDataSyncService syncService, ILogger<FlightSyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SyncFlights(CancellationToken cancellationToken)
    {
        var result = await _syncService.SyncFlightsAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(new { Message = result.Message });
    }

    [HttpPost("date-range")]
    public async Task<IActionResult> SyncFlightsByDateRange(
        [FromBody] DateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _syncService.SyncFlightsByDateRangeAsync(
            request.StartDate, 
            request.EndDate, 
            cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(new { Message = result.Message });
    }
}
