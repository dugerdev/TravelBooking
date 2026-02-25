using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TravelBooking.Application.Abstractions.External;
using TravelBooking.Domain.Enums;
using System.Linq;

namespace TravelBooking.Application.Services;

/// <summary>
/// Ucuslere iliskin is kurallarini yoneten servis.
/// Ucus olusturma, guncelleme, arama ve dis API entegrasyonu islemlerini yonetir.
/// </summary>
public class FlightManager : IFlightService
{
    private readonly IUnitOfWork _unitOfWork;                                        //---Tum repository'leri yoneten yapi---//
    private readonly IValidator<Flight> _validator;                                   //---Ucus dogrulama kurallari---//
    private readonly ILogger<FlightManager> _logger;                                  //---Logging servisi---//
    private readonly IMemoryCache _cache;                                            //---Memory cache---//
    private readonly IExternalFlightApiClient? _externalFlightApi;                   //---Dis API client (opsiyonel)---//
    private const string CacheKeyPrefix = "flight_";                                 //---Cache key prefix---//
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);     //---Cache expiration suresi (ucuslar daha sik degisebilir)---//

    /// <summary>
    /// FlightManager constructor.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work instance.</param>
    /// <param name="validator">Ucus validator.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="cache">Memory cache instance.</param>
    /// <param name="externalFlightApi">Dis ucus API client (opsiyonel).</param>
    public FlightManager(
        IUnitOfWork unitOfWork, 
        IValidator<Flight> validator, 
        ILogger<FlightManager> logger,
        IMemoryCache cache,
        IExternalFlightApiClient? externalFlightApi = null)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _cache = cache;
        _externalFlightApi = externalFlightApi;
    }

    /// <summary>
    /// ID'ye gore ucus getirir. DepartureAirport ve ArrivalAirport navigation property'lerini de yukler.
    /// </summary>
    /// <param name="id">Ucus ID'si.</param>
    /// <param name="cancellationToken">Iptal token'i.</param>
    /// <returns>Ucus bilgileri veya hata mesaji.</returns>
    public async Task<DataResult<Flight>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting flight by id: {FlightId}", id);
        
        var cacheKey = $"{CacheKeyPrefix}id_{id}";
        
        //---Cache'den kontrol et---//
        if (_cache.TryGetValue(cacheKey, out Flight? cachedFlight) && cachedFlight != null)
        {
            _logger.LogDebug("Flight retrieved from cache: {FlightId}", id);
            return new SuccessDataResult<Flight>(cachedFlight);
        }
        
        //---Soft delete edilmemis ucusu getir (navigation properties ile)---//
        var flight = await _unitOfWork.Context.Set<Flight>()
            .Where(f => f.Id == id && !f.IsDeleted)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .FirstOrDefaultAsync(cancellationToken);

        if (flight is null)
        {
            _logger.LogWarning("Flight not found with id: {FlightId}", id);
            return new ErrorDataResult<Flight>(null!, "Ucus bulunamadi.");
        }

        //---Cache'e ekle---//
        _cache.Set(cacheKey, flight, CacheExpiration);
        _logger.LogDebug("Flight cached: {FlightId}", id);

        _logger.LogInformation("Flight found: {FlightId} - {FlightNumber}", flight.Id, flight.FlightNumber);
        return new SuccessDataResult<Flight>(flight);
    }

    //---Tum ucuslari getiren metot (admin listesi icin havalimanlari dahil)---//
    public async Task<DataResult<IEnumerable<Flight>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var flights = await _unitOfWork.Context.Set<Flight>()
            .Where(f => !f.IsDeleted)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .OrderBy(f => f.ScheduledDeparture)
            .ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Flight>>(flights);
    }

    //---Tum ucuslari pagination ile getiren metot---//
    public async Task<DataResult<PagedResult<Flight>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting flights with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Flights.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Flight>>(pagedResult);
    }

    //---Kalkis havalimanina gore ucuslari getiren metot---//
    public async Task<DataResult<IEnumerable<Flight>>> GetByDepartureAirportAsync(Guid airportId, CancellationToken cancellationToken = default)
    {
        // Include navigation properties to avoid N+1 queries
        var flights = await _unitOfWork.Context.Set<Flight>()
            .Where(f => f.DepartureAirportId == airportId && !f.IsDeleted)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Flight>>(flights);
    }

    //---Varis havalimanina gore ucuslari getiren metot---//
    public async Task<DataResult<IEnumerable<Flight>>> GetByArrivalAirportAsync(Guid airportId, CancellationToken cancellationToken = default)
    {
        // Include navigation properties to avoid N+1 queries
        var flights = await _unitOfWork.Context.Set<Flight>()
            .Where(f => f.ArrivalAirportId == airportId && !f.IsDeleted)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Flight>>(flights);
    }

    //---Yeni ucus ekleyen metot---//
    public async Task<Result> AddAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new flight: {FlightNumber}", flight.FlightNumber);
        
        try
        {
            await _validator.ValidateAndThrowAsync(flight);                               //---FluentValidation kontrolu---//

            await _unitOfWork.Flights.AddAsync(flight, cancellationToken);                                  //---Repository uzerinden ekle---//
            await _unitOfWork.SaveChangesAsync(cancellationToken);                                        //---Kalici hale getir---//

            //---Cache'e ekle---//
            var cacheKey = $"{CacheKeyPrefix}id_{flight.Id}";
            _cache.Set(cacheKey, flight, CacheExpiration);
            _logger.LogDebug("Flight cached: {FlightId}", flight.Id);

            _logger.LogInformation("Flight added successfully: {FlightId} - {FlightNumber}", flight.Id, flight.FlightNumber);
            return new SuccessResult("Ucus eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            _logger.LogError(dbEx, "Database error while adding flight: {FlightNumber}. Inner: {InnerMessage}", 
                flight.FlightNumber, innerMessage);
            
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 547) // Foreign key constraint
                    return new ErrorResult($"Ucus eklenirken hata: Havalimani bulunamadi. SQL: {sqlEx.Message}");
                else if (sqlEx.Number == 515)
                    return new ErrorResult($"Ucus eklenirken hata: Zorunlu alan eksik. SQL: {sqlEx.Message}");
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    return new ErrorResult($"Ucus eklenirken hata: Bu ucus numarasi zaten mevcut. SQL: {sqlEx.Message}");
            }
            
            return new ErrorResult($"Ucus eklenirken veritabani hatasi: {innerMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding flight: {FlightNumber}", flight.FlightNumber);
            return new ErrorResult($"Ucus eklenirken hata olustu: {ex.Message}");
        }
    }

    //---Mevcut ucusu guncelleyen metot---//
    public async Task<Result> UpdateAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(flight);

            await _unitOfWork.Flights.UpdateAsync(flight, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //---Cache'i guncelle---//
            var cacheKey = $"{CacheKeyPrefix}id_{flight.Id}";
            _cache.Set(cacheKey, flight, CacheExpiration);
            _logger.LogDebug("Flight cache updated: {FlightId}", flight.Id);

            return new SuccessResult("Ucus guncellendi.");
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed: {FlightId}", flight.Id);
            return new ErrorResult($"Validation hatasi: {string.Join(", ", ex.Errors.Select(e => e.ErrorMessage))}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight: {FlightId}", flight.Id);
            return new ErrorResult($"Ucus guncellenirken hata olustu: {ex.Message}");
        }
    }

    //---Ucusu soft delete eden metot---//
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.Flights.SoftDeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //---Cache'i temizle---//
            var cacheKey = $"{CacheKeyPrefix}id_{id}";
            _cache.Remove(cacheKey);
            _logger.LogDebug("Flight deleted and cache cleared: {FlightId}", id);

            return new SuccessResult("Ucus silindi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight: {FlightId}", id);
            return new ErrorResult($"Ucus silinirken hata olustu: {ex.Message}");
        }
    }

    //---Flight number ve tarihe gore ucus getiren metot---//
    public async Task<DataResult<Flight>> GetByFlightNumberAsync(string flightNumber, DateTime scheduledDeparture, CancellationToken cancellationToken = default)
    {
        var flights = await _unitOfWork.Flights.FindAsync(f => 
            f.FlightNumber == flightNumber && 
            f.ScheduledDeparture.Date == scheduledDeparture.Date, cancellationToken);
        
        var flight = flights.FirstOrDefault();
        if (flight is null)
            return new ErrorDataResult<Flight>(null!, "Ucus bulunamadi.");

        return new SuccessDataResult<Flight>(flight);
    }

    //---Birden fazla ucus ekleyen metot---//
    public async Task<Result> AddRangeAsync(IEnumerable<Flight> flights, CancellationToken cancellationToken = default)
    {
        var flightList = flights.ToList();
        foreach (var flight in flightList)
        {
            await _validator.ValidateAndThrowAsync(flight);
        }

        await _unitOfWork.Flights.AddRangeAsync(flightList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult($"{flightList.Count} ucus eklendi.");
    }

    //---Upsert metot (varsa update, yoksa insert)---//
    public async Task<Result> UpsertAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(flight);

        var existingFlight = (await GetByFlightNumberAsync(flight.FlightNumber, flight.ScheduledDeparture, cancellationToken)).Data;
        
        if (existingFlight != null)
        {
            var dep = flight.ScheduledDeparture;
            var arr = flight.ScheduledArrival;
            if (arr <= dep)
            {
                arr = dep.AddHours(1);
                _logger.LogWarning("Upsert: varis <= kalkis; varis 1 saat sonraya ayarlandi. FlightNumber={Fn}", flight.FlightNumber);
            }
            existingFlight.UpdateSchedule(dep, arr);
            
            // Update AvailableSeats if provided (for external API sync)
            if (flight.AvailableSeats != existingFlight.AvailableSeats)
            {
                existingFlight.UpdateAvailableSeats(flight.AvailableSeats);
            }
            
            await _unitOfWork.Flights.UpdateAsync(existingFlight, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            //---Cache'i guncelle---//
            var cacheKey = $"{CacheKeyPrefix}id_{existingFlight.Id}";
            _cache.Set(cacheKey, existingFlight, CacheExpiration);
            _logger.LogDebug("Flight cache updated: {FlightId}", existingFlight.Id);
            
            return new SuccessResult("Ucus guncellendi.");
        }
        else
        {
            // Add new flight
            await _unitOfWork.Flights.AddAsync(flight, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            //---Cache'e ekle---//
            var cacheKey = $"{CacheKeyPrefix}id_{flight.Id}";
            _cache.Set(cacheKey, flight, CacheExpiration);
            _logger.LogDebug("Flight cached: {FlightId}", flight.Id);
            
            return new SuccessResult("Ucus eklendi.");
        }
    }

    //---Hibrit arama: Once veritabani, sonra dis API---//
    public async Task<DataResult<IEnumerable<Flight>>> SearchFlightsHybridAsync(
        Guid departureAirportId, 
        Guid arrivalAirportId, 
        DateTime departureDate, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Hibrit ucus aramasi: {Departure} → {Arrival}, Tarih: {Date}", 
            departureAirportId, arrivalAirportId, departureDate.Date);

        //---1. Once veritabaninda ara (navigation properties ile)---//
        var dbFlights = await _unitOfWork.Context.Set<Flight>()
            .Where(f => f.DepartureAirportId == departureAirportId &&
                        f.ArrivalAirportId == arrivalAirportId &&
                        f.ScheduledDeparture.Date == departureDate.Date &&
                        !f.IsDeleted)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .ToListAsync(cancellationToken);

        var flightsList = dbFlights.ToList();

        if (flightsList.Any())
        {
            _logger.LogInformation("Veritabaninda {Count} ucus bulundu. Dis API cagrisi yapilmadi.", flightsList.Count);
            return new SuccessDataResult<IEnumerable<Flight>>(flightsList, 
                $"{flightsList.Count} ucus bulundu (Veritabani).");
        }

        //---2. Veritabaninda yoksa dis API'den cek---//
        if (_externalFlightApi == null)
        {
            _logger.LogWarning("Veritabaninda ucus bulunamadi ve dis API servisi yapilandirilmamis.");
            return new SuccessDataResult<IEnumerable<Flight>>(Enumerable.Empty<Flight>(), 
                "Ucus bulunamadi. Dis API servisi yapilandirilmamis.");
        }

        _logger.LogInformation("Veritabaninda ucus bulunamadi. Dis API'den cekiliyor...");

        try
        {
            //---Dis API'den ucuslari cek---//
            var externalFlights = await _externalFlightApi.GetFlightsByDateRangeAsync(
                departureDate.Date, 
                departureDate.Date.AddDays(1), 
                cancellationToken);

            if (!externalFlights.Any())
            {
                _logger.LogInformation("Dis API'den ucus bulunamadi.");
                return new SuccessDataResult<IEnumerable<Flight>>(Enumerable.Empty<Flight>(), 
                    "Belirtilen kriterlere uygun ucus bulunamadi.");
            }

            //---Havalimani IATA kodlarini al---//
            var departureAirport = await _unitOfWork.Airports.GetByIdAsync(departureAirportId, cancellationToken);
            var arrivalAirport = await _unitOfWork.Airports.GetByIdAsync(arrivalAirportId, cancellationToken);

            if (departureAirport == null || arrivalAirport == null)
            {
                _logger.LogWarning("Havalimani bulunamadi: Departure={DepartureId}, Arrival={ArrivalId}", 
                    departureAirportId, arrivalAirportId);
                return new ErrorDataResult<IEnumerable<Flight>>(Enumerable.Empty<Flight>(), 
                    "Havalimani bilgisi bulunamadi.");
            }

            //---Filtreleme: Sadece istenen rota---//
            var matchingFlights = externalFlights.Where(ef =>
                ef.DepartureAirportIATA == departureAirport.IATA_Code &&
                ef.ArrivalAirportIATA == arrivalAirport.IATA_Code).ToList();

            if (!matchingFlights.Any())
            {
                _logger.LogInformation("Dis API'den {Total} ucus cekildi ama istenen rotada ucus yok.", externalFlights.Count());
                return new SuccessDataResult<IEnumerable<Flight>>(Enumerable.Empty<Flight>(), 
                    "Belirtilen rotada ucus bulunamadi.");
            }

            //---External DTO'yu Domain Entity'ye donustur ve veritabanina kaydet---//
            var newFlights = new List<Flight>();
            foreach (var externalFlight in matchingFlights)
            {
                var dep = externalFlight.ScheduledDeparture;
                var arr = externalFlight.ScheduledArrival;
                if (arr <= dep)
                {
                    arr = dep.AddHours(1);
                    _logger.LogDebug("Dis API ucusunda varis <= kalkis; varis 1 saat sonraya ayarlandi. FlightNumber={Fn}", externalFlight.FlightNumber);
                }

                var currency = ParseCurrency(externalFlight.Currency);
                var flightType = ParseFlightType(externalFlight.FlightType);
                var flightRegion = ParseFlightRegion(externalFlight.FlightRegion);

                var flight = new Flight(
                    externalFlight.FlightNumber ?? "UNKNOWN",
                    externalFlight.AirlineName ?? "Unknown Airline",
                    departureAirportId,
                    arrivalAirportId,
                    dep,
                    arr,
                    new Money(externalFlight.BasePriceAmount > 0 ? externalFlight.BasePriceAmount : 1000, currency),
                    externalFlight.TotalSeats > 0 ? externalFlight.TotalSeats : 180,
                    flightType,
                    flightRegion
                );

                newFlights.Add(flight);
            }

            //---Veritabanina kaydet---//
            await _unitOfWork.Flights.AddRangeAsync(newFlights, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Dis API'den {Count} ucus cekildi ve veritabanina kaydedildi.", newFlights.Count);

            return new SuccessDataResult<IEnumerable<Flight>>(newFlights, 
                $"{newFlights.Count} ucus bulundu (Dis API'den cekildi ve kaydedildi).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dis API'den ucus cekilirken hata olustu: {Message}", ex.Message);
            return new ErrorDataResult<IEnumerable<Flight>>(Enumerable.Empty<Flight>(), 
                $"Dis API hatasi: {ex.Message}");
        }
    }

    //---IATA listeleri ve tarih ile veritabaninda ucus arama (search-external DB fallback)---//
    public async Task<DataResult<IEnumerable<Flight>>> SearchFlightsByIataAndDateAsync(IEnumerable<string> fromIatas, IEnumerable<string> toIatas, DateTime date, CancellationToken cancellationToken = default)
    {
        var fromList = fromIatas?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim().ToUpperInvariant()).Distinct().ToList() ?? new List<string>();
        var toList = toIatas?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim().ToUpperInvariant()).Distinct().ToList() ?? new List<string>();
        if (!fromList.Any() || !toList.Any())
            return new SuccessDataResult<IEnumerable<Flight>>(Array.Empty<Flight>());

        // Optimized query with proper includes to avoid N+1
        var list = await _unitOfWork.Context.Set<Flight>()
            .Where(f => !f.IsDeleted &&
                fromList.Contains(f.DepartureAirport.IATA_Code) &&
                toList.Contains(f.ArrivalAirport.IATA_Code) &&
                f.ScheduledDeparture.Date == date.Date)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .OrderBy(f => f.ScheduledDeparture)
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<IEnumerable<Flight>>(list);
    }

    //---Helper metodlar: String'den enum'a donusturme---//
    private static Currency ParseCurrency(string currency)
    {
        return currency.ToUpperInvariant() switch
        {
            "TRY" => Currency.TRY,
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            _ => Currency.TRY
        };
    }

    private static FlightType ParseFlightType(string flightType)
    {
        return flightType.ToUpperInvariant() switch
        {
            "DIRECT" => FlightType.Direct,
            "CONNECTING" => FlightType.Connecting,
            "TRANSIT" => FlightType.Connecting,
            "CHARTER" => FlightType.Charter,
            _ => FlightType.Direct
        };
    }

    private static FlightRegion ParseFlightRegion(string flightRegion)
    {
        return flightRegion.ToUpperInvariant() switch
        {
            "DOMESTIC" => FlightRegion.Domestic,
            "INTERNATIONAL" => FlightRegion.International,
            _ => FlightRegion.Domestic
        };
    }
}

