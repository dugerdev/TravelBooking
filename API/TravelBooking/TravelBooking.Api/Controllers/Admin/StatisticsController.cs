using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/statistics")]
[Authorize(Roles = "Admin")]
public sealed class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DataResult<DashboardStatisticsDto>>> GetDashboardStatistics(CancellationToken cancellationToken)
    {
        var result = await _statisticsService.GetDashboardStatisticsAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("reservations")]
    public async Task<ActionResult<DataResult<ReservationStatisticsDto>>> GetReservationStatistics(CancellationToken cancellationToken)
    {
        var result = await _statisticsService.GetReservationStatisticsAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<DataResult<RevenueStatisticsDto>>> GetRevenueStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var result = await _statisticsService.GetRevenueStatisticsAsync(startDate, endDate, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<DataResult<UserStatisticsDto>>> GetUserStatistics(CancellationToken cancellationToken)
    {
        var result = await _statisticsService.GetUserStatisticsAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
