using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Dtos.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelBooking.Api.Controllers;

//---Dis API test endpoint'leri icin controller---//
//---Bu controller sadece test amaclidir, veritabanina yazmaz. Sadece Admin erisebilir (dis API kotasi korunur).---//
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/flights/test-external")]
[SwaggerTag("Dis API (Aviationstack) test endpoint'leri — Sadece Admin")]
public class FlightsTestController : ControllerBase
{
    private readonly IExternalFlightApiClient _externalApiClient;
    private readonly ILogger<FlightsTestController> _logger;

    public FlightsTestController(
        IExternalFlightApiClient externalApiClient,
        ILogger<FlightsTestController> logger)
    {
        _externalApiClient = externalApiClient;
        _logger = logger;
    }

    //---Aviationstack API'den ucus verilerini ceker (test amacli)---//
    [HttpGet]
    [SwaggerOperation(
        Summary = "Dis API'den ucus verilerini test et",
        Description = "Aviationstack API'sinden ucus verilerini ceker ve dondurur. Veritabanina kaydetmez. Sadece test amaclidir.")]
    [ProducesResponseType(typeof(IEnumerable<ExternalFlightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ExternalFlightDto>>> TestExternalApi(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing external API connection. Limit: {Limit}", limit);

            //---Dis API'den veri cek---//
            var flights = await _externalApiClient.GetFlightsAsync(cancellationToken);
            
            //---Limit uygula (eger belirtilmisse)---//
            var limitedFlights = flights.Take(limit).ToList();

            _logger.LogInformation("Successfully fetched {Count} flights from external API", limitedFlights.Count);

            return Ok(new
            {
                Success = true,
                Message = $"Basariyla {limitedFlights.Count} ucus verisi cekildi.",
                TotalFetched = flights.Count(),
                Returned = limitedFlights.Count,
                Data = limitedFlights
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching from external API");
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Title = "Dis API Baglanti Hatasi",
                    Detail = $"Aviationstack API'ye baglanilamadi: {ex.Message}",
                    Status = StatusCodes.Status502BadGateway
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while testing external API");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Beklenmeyen Hata",
                    Detail = $"Dis API testi sirasinda hata olustu: {ex.Message}",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    //---Tarih araligi ile dis API'den ucus verilerini ceker---//
    [HttpGet("date-range")]
    [SwaggerOperation(
        Summary = "Tarih araligi ile dis API'den ucus verilerini test et",
        Description = "Belirtilen tarih araliginda Aviationstack API'sinden ucus verilerini ceker. Veritabanina kaydetmez.")]
    [ProducesResponseType(typeof(IEnumerable<ExternalFlightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ExternalFlightDto>>> TestExternalApiByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Gecersiz Tarih Araligi",
                    Detail = "Baslangic tarihi bitis tarihinden sonra olamaz.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if ((endDate - startDate).Days > 30)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Tarih Araligi Cok Genis",
                    Detail = "Tarih araligi en fazla 30 gun olabilir.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation("Testing external API connection for date range: {StartDate} to {EndDate}", startDate, endDate);

            //---Dis API'den veri cek---//
            var flights = await _externalApiClient.GetFlightsByDateRangeAsync(startDate, endDate, cancellationToken);
            var flightsList = flights.ToList();

            _logger.LogInformation("Successfully fetched {Count} flights from external API for date range", flightsList.Count);

            return Ok(new
            {
                Success = true,
                Message = $"Basariyla {flightsList.Count} ucus verisi cekildi.",
                StartDate = startDate,
                EndDate = endDate,
                TotalFetched = flightsList.Count,
                Data = flightsList
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching from external API");
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Title = "Dis API Baglanti Hatasi",
                    Detail = $"Aviationstack API'ye baglanilamadi: {ex.Message}",
                    Status = StatusCodes.Status502BadGateway
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while testing external API");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Beklenmeyen Hata",
                    Detail = $"Dis API testi sirasinda hata olustu: {ex.Message}",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
}
