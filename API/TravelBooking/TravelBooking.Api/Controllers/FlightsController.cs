using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Application.Dtos.External;
using TravelBooking.Application.Abstractions.External;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Linq;

namespace TravelBooking.Api.Controllers;

//---Ucus islemleri icin controller---//
[Route("api/[controller]")]
[SwaggerTag("Ucus islemleri icin endpoint'ler")]
public class FlightsController : BaseController
{
    private readonly IFlightService _flightService;
    private readonly IAirportService _airportService;
    private readonly IMapper _mapper;
    private readonly IExternalFlightApiClient _externalApiClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FlightsController> _logger;
    private readonly IFlightDataSyncService _flightDataSyncService;
    private readonly IConfiguration _configuration;
    private readonly IPricingPolicy _pricingPolicy;

    public FlightsController(
        IFlightService flightService,
        IAirportService airportService,
        IMapper mapper,
        IExternalFlightApiClient externalApiClient,
        IMemoryCache cache,
        ILogger<FlightsController> logger,
        IFlightDataSyncService flightDataSyncService,
        IConfiguration configuration,
        IPricingPolicy pricingPolicy)
    {
        _flightService = flightService;
        _airportService = airportService;
        _mapper = mapper;
        _externalApiClient = externalApiClient;
        _cache = cache;
        _logger = logger;
        _flightDataSyncService = flightDataSyncService;
        _configuration = configuration;
        _pricingPolicy = pricingPolicy;
    }

    //---Yeni ucus ekle---//
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Yeni ucus ekle", Description = "Yeni bir ucus kaydi olusturur. Sadece Admin.")]
    [ProducesResponseType(typeof(SuccessDataResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> Create([FromBody] CreateFlightDto dto, CancellationToken cancellationToken = default)
    {
        var flight = new Flight(
            dto.FlightNumber,
            dto.AirlineName,
            dto.DepartureAirportId,
            dto.ArrivalAirportId,
            dto.ScheduledDeparture,
            dto.ScheduledArrival,
            new Money(dto.BasePriceAmount, dto.Currency),
            dto.TotalSeats,
            dto.FlightType,
            dto.FlightRegion);

        var result = await _flightService.AddAsync(flight, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(new SuccessDataResult<Guid>(flight.Id, result.Message));
    }

    //---Tum ucuslari getir---//
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Tum ucuslari getir", Description = "Tum ucuslari listeler. Pagination destegi vardir. Giris gerekmez.")]
    public async Task<ActionResult<DataResult<IEnumerable<FlightDto>>>> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        // Pagination varsa paginated endpoint kullan
        if (request != null)
        {
            var pagedResult = await _flightService.GetAllPagedAsync(request, cancellationToken);
            return HandlePagedResult<Flight, FlightDto>(pagedResult, _mapper);
        }

        var result = await _flightService.GetAllAsync(cancellationToken);
        return HandleListResult<Flight, FlightDto>(result, _mapper);
    }

    //---ID'ye gore ucus getir---//
    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "ID'ye gore ucus getir", Description = "Belirtilen ID'ye sahip ucus bilgilerini getirir. Giris gerekmez.")]
    public async Task<ActionResult<DataResult<FlightDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _flightService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var flightDto = _mapper.Map<FlightDto>(result.Data);

        return Ok(new SuccessDataResult<FlightDto>(flightDto));
    }

    //Örnek: Koltuk sinifi (Economy/Business) ve bagaj secenegine (Light/Heavy) gore bilet+bagaj fiyati hesaplanir
    [HttpGet("{id:guid}/quote")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Ucus fiyat teklifi", Description = "Koltuk sinifi ve bagaj secenegine gore bilet+bagaj fiyatini hesaplar. Varsayilan: Economy, Light. Giris gerekmez.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetQuote(
        Guid id,
        [FromQuery] SeatClass? seatClass = null,
        [FromQuery] BaggageOption? baggageOption = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _flightService.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound(new ErrorResult("Ucus bulunamadi."));

        var flight = result.Data;
        var sc = seatClass ?? SeatClass.Economy;
        var bo = baggageOption ?? BaggageOption.Light;

        var (ticketPrice, baggageFee) = _pricingPolicy.CalculateTicketPriceAndBaggage(flight, sc, bo);
        var totalForOneTicket = ticketPrice + baggageFee;
        var currency = flight.BasePrice?.Currency ?? Currency.TRY;

        return Ok(new { ticketPrice, baggageFee, totalForOneTicket, currency });
    }

    //---Kalkis havalimanina gore ucuslari getir---//
    [HttpGet("departure/{airportId}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Kalkis havalimanina gore ucuslari getir", Description = "Belirtilen kalkis havalimanina gore ucuslari listeler. Giris gerekmez.")]
    public async Task<ActionResult<DataResult<IEnumerable<FlightDto>>>> GetByDepartureAirport(Guid airportId, CancellationToken cancellationToken = default)
    {
        var result = await _flightService.GetByDepartureAirportAsync(airportId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Ucus verisi bulunamadi.");

        var flightDtos = _mapper.Map<IEnumerable<FlightDto>>(result.Data);

        return Ok(new SuccessDataResult<IEnumerable<FlightDto>>(flightDtos));
    }

    //---Varis havalimanina gore ucuslari getir---//
    [HttpGet("arrival/{airportId}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Varis havalimanina gore ucuslari getir", Description = "Belirtilen varis havalimanina gore ucuslari listeler. Giris gerekmez.")]
    public async Task<ActionResult<DataResult<IEnumerable<FlightDto>>>> GetByArrivalAirport(Guid airportId, CancellationToken cancellationToken = default)
    {
        var result = await _flightService.GetByArrivalAirportAsync(airportId, cancellationToken);
        return HandleListResult<Flight, FlightDto>(result, _mapper);
    }

    //Örnek: Hibrit arama - from/to IATA veya isimle arar; coklu havalimani kombinasyonlari icin HashSet ile tekrar eden sonuclar filtrelenir
    [HttpGet("search")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Ucus ara (Hibrit)",
        Description = "from/to (IATA veya ad) veya departureAirportId/arrivalAirportId ile aranabilir. Once veritabani, yoksa dis API. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<FlightDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<IEnumerable<FlightDto>>>> SearchHybrid(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] Guid departureAirportId,
        [FromQuery] Guid arrivalAirportId,
        [FromQuery] DateTime departureDate,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
        {
            var fromRes = await _airportService.GetIataCodesByNameOrIataAsync(from!.Trim(), 10, cancellationToken);
            var toRes = await _airportService.GetIataCodesByNameOrIataAsync(to!.Trim(), 10, cancellationToken);
            if (!fromRes.Success || fromRes.Data == null || !fromRes.Data.Any())
                return BadRequest(new ErrorResult("Kalkis havalimani bulunamadi."));
            if (!toRes.Success || toRes.Data == null || !toRes.Data.Any())
                return BadRequest(new ErrorResult("Varis havalimani bulunamadi."));

            var fromIds = new List<Guid>();
            foreach (var iata in fromRes.Data)
            {
                var a = await _airportService.GetByIATACodeAsync(iata, cancellationToken);
                if (a.Success && a.Data != null) fromIds.Add(a.Data.Id);
            }
            var toIds = new List<Guid>();
            foreach (var iata in toRes.Data)
            {
                var a = await _airportService.GetByIATACodeAsync(iata, cancellationToken);
                if (a.Success && a.Data != null) toIds.Add(a.Data.Id);
            }
            if (fromIds.Count == 0 || toIds.Count == 0)
                return BadRequest(new ErrorResult("Kalkis veya varis havalimani bulunamadi."));

            //Örnek: Ayni ucusun birden fazla rota kombinasyonunda tekrar gelmesini engeller
            var seen = new HashSet<Guid>();
            var list = new List<Flight>();
            foreach (var depId in fromIds)
            foreach (var arrId in toIds)
            {
                var r = await _flightService.SearchFlightsHybridAsync(depId, arrId, departureDate, cancellationToken);
                if (r.Success && r.Data != null)
                    foreach (var f in r.Data)
                        if (seen.Add(f.Id)) list.Add(f);
            }
            var dtos = _mapper.Map<IEnumerable<FlightDto>>(list.OrderBy(f => f.ScheduledDeparture));
            return Ok(new SuccessDataResult<IEnumerable<FlightDto>>(dtos, $"{list.Count} ucus bulundu."));
        }

        if (departureAirportId == Guid.Empty && arrivalAirportId == Guid.Empty)
            return BadRequest(new ErrorResult("departureAirportId ve arrivalAirportId veya from ve to zorunludur."));

        var result = await _flightService.SearchFlightsHybridAsync(departureAirportId, arrivalAirportId, departureDate, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        if (result.Data == null)
            return NotFoundError("Ucus verisi bulunamadi.");
        var flightDtos = _mapper.Map<IEnumerable<FlightDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<FlightDto>>(flightDtos, result.Message));
    }

    //---Dis API'den ucus ara (Frontend icin); giris yapmadan kullanilabilir---//
    [HttpGet("search-external")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("external-search")]
    [SwaggerOperation(
        Summary = "Dis API'den ucus ara",
        Description = "Aviationstack API'den gercek zamanli ucus verilerini arar. Kalkis, varis ve tarih parametrelerine gore filtreler. Sonuclar 5 dakika cache'lenir. Rate limit: 10 istek/dakika. Sadece sistemde kayitli IATA kodlari (veya ad/sehir) kabul edilir; kayitsiz kodlar (orn. DbSeeder'da olmayan) 400 doner.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<ExternalFlightDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<DataResult<IEnumerable<ExternalFlightDto>>>> SearchExternal(
        [FromQuery, SwaggerParameter("Kalkis: IATA (IST) veya ad/sehir (Antalya, Istanbul Havalimani)", Required = true)] string from,
        [FromQuery, SwaggerParameter("Varis: IATA (AYT) veya ad/sehir", Required = true)] string to,
        [FromQuery, SwaggerParameter("Kalkis tarihi (YYYY-MM-DD)", Required = true)] DateTime date,
        [FromQuery, SwaggerParameter("Maksimum sonuc (varsayilan: 20)", Required = false)] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            return BadRequest(new ErrorResult("Kalkis ve varis havalimani zorunludur."));
        if (date.Date < DateTime.UtcNow.Date)
            return BadRequest(new ErrorResult("Gecmis tarihli ucus aramasi yapilamaz."));
        if (date.Day > DateTime.DaysInMonth(date.Year, date.Month))
            return BadRequest(new ErrorResult("Belirtilen tarih gecerli degil (or. 2026-02-29)."));
        if (limit < 1 || limit > 100)
            return BadRequest(new ErrorResult("Limit 1-100 arasinda olmalidir."));

        var fromRes = await _airportService.GetIataCodesByNameOrIataAsync(from.Trim(), 10, cancellationToken);
        var toRes = await _airportService.GetIataCodesByNameOrIataAsync(to.Trim(), 10, cancellationToken);
        if (!fromRes.Success || fromRes.Data == null || !fromRes.Data.Any())
            return BadRequest(new ErrorResult("Kalkis havalimani bulunamadi."));
        if (!toRes.Success || toRes.Data == null || !toRes.Data.Any())
            return BadRequest(new ErrorResult("Varis havalimani bulunamadi."));

        var fromIatas = fromRes.Data.ToList();
        var toIatas = toRes.Data.ToList();
        //Örnek: Cache key - from/to IATA kombinasyonu + tarih ile 5 dakika cache (dis API maliyetini azaltir)
        var fromKey = string.Join("_", fromIatas.OrderBy(x => x));
        var toKey = string.Join("_", toIatas.OrderBy(x => x));
        var cacheKey = $"external_flights_{fromKey}_{toKey}_{date:yyyy-MM-dd}";

        if (_cache.TryGetValue(cacheKey, out List<ExternalFlightDto>? cachedFlights))
        {
            _logger.LogInformation("External flight search from cache. From: {From}, To: {To}, Date: {Date}, Results: {Count}", from, to, date, cachedFlights!.Count);
            var limited = cachedFlights.Take(limit).ToList();
            return Ok(new SuccessDataResult<IEnumerable<ExternalFlightDto>>(limited, $"{limited.Count} ucus bulundu (cache'den)"));
        }

        _logger.LogInformation("External flight search started. From: {From}, To: {To}, Date: {Date}", from, to, date);

        List<ExternalFlightDto> merged = new();

        try
        {
            if (!string.IsNullOrWhiteSpace(_configuration["AeroDataBox:RapidAPIKey"]))
            {
                //Örnek: Tum kalkis-varis IATA kombinasyonlari icin dis API cagrilir; tekrarlar ExternalFlightId ile giderilir
                foreach (var fi in fromIatas)
                foreach (var ti in toIatas)
                {
                    var ext = await _externalApiClient.GetFlightsFilteredAsync(fi, ti, date, cancellationToken);
                    merged.AddRange(ext);
                }
                merged = merged.GroupBy(x => x.ExternalFlightId).Select(g => g.First()).ToList();
            }

            //Örnek: Dis API bos donerse veya hata verirse veritabanindan fallback; ayni rota icin kayitli ucuslar kullanilir
            if (merged.Count == 0)
            {
                var dbRes = await _flightService.SearchFlightsByIataAndDateAsync(fromIatas, toIatas, date, cancellationToken);
                if (dbRes.Success && dbRes.Data != null && dbRes.Data.Any())
                    merged = _mapper.Map<List<ExternalFlightDto>>(dbRes.Data);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error from external API. From: {From}, To: {To}, Date: {Date}", from, to, date);
            var dbRes = await _flightService.SearchFlightsByIataAndDateAsync(fromIatas, toIatas, date, cancellationToken);
            if (dbRes.Success && dbRes.Data != null && dbRes.Data.Any())
                merged = _mapper.Map<List<ExternalFlightDto>>(dbRes.Data);
            else
                return Ok(new SuccessDataResult<IEnumerable<ExternalFlightDto>>(Array.Empty<ExternalFlightDto>(), "Dis API su an kullanilamiyor ve bu rota/tarih icin kayitli ucus yok."));
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout from external API. From: {From}, To: {To}, Date: {Date}", from, to, date);
            var dbRes = await _flightService.SearchFlightsByIataAndDateAsync(fromIatas, toIatas, date, cancellationToken);
            if (dbRes.Success && dbRes.Data != null && dbRes.Data.Any())
                merged = _mapper.Map<List<ExternalFlightDto>>(dbRes.Data);
            else
                return Ok(new SuccessDataResult<IEnumerable<ExternalFlightDto>>(Array.Empty<ExternalFlightDto>(), "Dis API su an kullanilamiyor ve bu rota/tarih icin kayitli ucus yok."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching external flights. From: {From}, To: {To}, Date: {Date}", from, to, date);
            var dbRes = await _flightService.SearchFlightsByIataAndDateAsync(fromIatas, toIatas, date, cancellationToken);
            if (dbRes.Success && dbRes.Data != null && dbRes.Data.Any())
                merged = _mapper.Map<List<ExternalFlightDto>>(dbRes.Data);
            else
                return Ok(new SuccessDataResult<IEnumerable<ExternalFlightDto>>(Array.Empty<ExternalFlightDto>(), "Dis API su an kullanilamiyor ve bu rota/tarih icin kayitli ucus yok."));
        }

        //Örnek: Sonuclar 5 dakika mutlak, 2 dakika sliding expiration ile cache'lenir
        var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), SlidingExpiration = TimeSpan.FromMinutes(2) };
        if (merged.Count > 0)
            _cache.Set(cacheKey, merged, cacheOptions);

        var limitedResults = merged.Take(limit).ToList();
        return Ok(new SuccessDataResult<IEnumerable<ExternalFlightDto>>(limitedResults, $"{limitedResults.Count} ucus bulundu"));
    }

    [HttpPost("import-from-external")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Dis ucusu ice aktar", Description = "search-external ile bulunan ExternalFlightDto'yu veritabanina alir, FlightId (Guid) doner. Sadece Admin.")]
    [ProducesResponseType(typeof(SuccessDataResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<Guid>>> ImportFromExternal(
        [FromBody] ExternalFlightDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.DepartureAirportIATA) || string.IsNullOrWhiteSpace(dto.ArrivalAirportIATA))
            return BadRequest(new ErrorResult("Kalkis ve varis IATA zorunludur."));
        if (dto.ScheduledDeparture == default)
            return BadRequest(new ErrorResult("ScheduledDeparture zorunludur."));
        var result = await _flightDataSyncService.ImportSingleExternalFlightAsync(dto, autoCreateAirports: true, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}

