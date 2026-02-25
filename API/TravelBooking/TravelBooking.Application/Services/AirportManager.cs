using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

//---Havalimanlarina iliskin is kurallarini yoneten servis---//
public class AirportManager : IAirportService
{
    private readonly IUnitOfWork _unitOfWork;                                        //---Tum repository'leri yoneten yapi---//
    private readonly IValidator<Airport> _validator;                                  //---Havalimani dogrulama kurallari---//
    private readonly ILogger<AirportManager> _logger;                                 //---Logging servisi---//
    private readonly IMemoryCache _cache;                                            //---Memory cache---//
    private const string CacheKeyPrefix = "airport_";                                //---Cache key prefix---//
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);      //---Cache expiration suresi---//

    public AirportManager(
        IUnitOfWork unitOfWork, 
        IValidator<Airport> validator, 
        ILogger<AirportManager> logger,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    //---ID'ye gore havalimani getiren metot---//
    public async Task<DataResult<Airport>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}id_{id}";
        
        //---Cache'den kontrol et---//
        if (_cache.TryGetValue(cacheKey, out Airport? cachedAirport) && cachedAirport != null)
        {
            _logger.LogDebug("Airport retrieved from cache: {AirportId}", id);
            return new SuccessDataResult<Airport>(cachedAirport);
        }

        var airport = await _unitOfWork.Airports.GetByIdAsync(id, cancellationToken);

        if (airport is null)
            return new ErrorDataResult<Airport>(null!, "Havalimani bulunamadi.");

        //---Cache'e ekle---//
        _cache.Set(cacheKey, airport, CacheExpiration);
        _logger.LogDebug("Airport cached: {AirportId}", id);

        return new SuccessDataResult<Airport>(airport);
    }

    //---Tum havalimanlarini getiren metot---//
    public async Task<DataResult<IEnumerable<Airport>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{CacheKeyPrefix}all";
        
        //---Cache'den kontrol et---//
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Airport>? cachedAirports) && cachedAirports != null)
        {
            _logger.LogDebug("All airports retrieved from cache");
            return new SuccessDataResult<IEnumerable<Airport>>(cachedAirports);
        }

        var airports = await _unitOfWork.Airports.GetAllAsync(cancellationToken);
        
        //---Cache'e ekle---//
        _cache.Set(cacheKey, airports, CacheExpiration);
        _logger.LogDebug("All airports cached");

        return new SuccessDataResult<IEnumerable<Airport>>(airports);
    }

    //---Tum havalimanlarini pagination ile getiren metot---//
    public async Task<DataResult<PagedResult<Airport>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting airports with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Airports.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Airport>>(pagedResult);
    }

    //---IATA koduna gore havalimani getiren metot---//
    public async Task<DataResult<Airport>> GetByIATACodeAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeIataCodeForLookup(iataCode);
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != 3)
            return new ErrorDataResult<Airport>(null!, "IATA kodu tam olarak 3 karakter olmalidir.");

        var upperIataCode = normalized.ToUpperInvariant();
        var cacheKey = $"{CacheKeyPrefix}iata_{upperIataCode}";
        
        //---Cache'den kontrol et---//
        if (_cache.TryGetValue(cacheKey, out Airport? cachedAirport) && cachedAirport != null)
        {
            _logger.LogDebug("Airport retrieved from cache by IATA: {IATACode}", upperIataCode);
            return new SuccessDataResult<Airport>(cachedAirport);
        }

        var airports = await _unitOfWork.Airports.FindAsync(a => a.IATA_Code == upperIataCode, cancellationToken);
        var airport = airports.FirstOrDefault();

        if (airport is null)
            return new ErrorDataResult<Airport>(null!, "Havalimani bulunamadi.");

        //---Cache'e ekle---//
        _cache.Set(cacheKey, airport, CacheExpiration);
        _logger.LogDebug("Airport cached by IATA: {IATACode}", upperIataCode);

        return new SuccessDataResult<Airport>(airport);
    }

    //---IATA, Name veya City ile arama (autocomplete icin)---//
    public async Task<DataResult<IEnumerable<Airport>>> SearchAsync(string query, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new SuccessDataResult<IEnumerable<Airport>>(Array.Empty<Airport>());

        var q = query.Trim();
        var take = limit <= 0 ? 20 : (limit > 100 ? 100 : limit);

        var set = _unitOfWork.Context.Set<Airport>();
        
        // Case-insensitive ve Turkce karakter destegi icin ToLower kullan
        var searchLower = q.ToLower();
        
        var list = await set
            .Where(a => !a.IsDeleted &&
                ((a.IATA_Code != null && a.IATA_Code.ToLower().Contains(searchLower)) ||
                 (a.Name != null && a.Name.ToLower().Contains(searchLower)) ||
                 (a.City != null && a.City.ToLower().Contains(searchLower))))
            // Once sehir adina gore tam eslesme, sonra IATA, sonra digerleri
            .OrderBy(a => a.City != null && a.City.ToLower() == searchLower ? 0 : 1)
            .ThenBy(a => a.IATA_Code != null && a.IATA_Code.ToLower() == searchLower ? 0 : 1)
            .ThenBy(a => a.City)
            .ThenBy(a => a.IATA_Code)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<IEnumerable<Airport>>(list);
    }

    //---Ad, sehir veya IATA ile IATA kodlari listesi doner---//
    public async Task<DataResult<IEnumerable<string>>> GetIataCodesByNameOrIataAsync(string nameOrIata, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nameOrIata))
            return new ErrorDataResult<IEnumerable<string>>(Array.Empty<string>(), "Arama ifadesi bos olamaz.");

        var q = nameOrIata.Trim();
        if (q.Length == 3 && q.All(char.IsLetter))
        {
            var byIata = await GetByIATACodeAsync(q.ToUpperInvariant(), cancellationToken);
            if (byIata.Success && byIata.Data != null)
                return new SuccessDataResult<IEnumerable<string>>(new[] { byIata.Data.IATA_Code });
        }

        var search = await SearchAsync(q, maxResults, cancellationToken);
        if (!search.Success || search.Data == null)
            return new ErrorDataResult<IEnumerable<string>>(Array.Empty<string>(), search.Message ?? "Havalimani bulunamadi.");

        var iatas = search.Data
            .Select(a => a.IATA_Code)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .Take(maxResults)
            .ToList();

        if (iatas.Count == 0)
            return new ErrorDataResult<IEnumerable<string>>(Array.Empty<string>(), "Havalimani bulunamadi.");

        return new SuccessDataResult<IEnumerable<string>>(iatas);
    }

    //---Yeni havalimani ekleyen metot---//
    public async Task<Result> AddAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new airport: {IATACode} - {Name}", airport.IATA_Code, airport.Name);
        
        try
        {
            await _validator.ValidateAndThrowAsync(airport);

            await _unitOfWork.Airports.AddAsync(airport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //---Cache'i temizle---//
            _cache.Remove($"{CacheKeyPrefix}all");
            _logger.LogDebug("Cache cleared for all airports");

            _logger.LogInformation("Airport added successfully: {AirportId} - {IATACode}", airport.Id, airport.IATA_Code);
            return new SuccessResult("Havalimani eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            _logger.LogError(dbEx, "Database error while adding airport: {IATACode} - {Name}. Inner: {InnerMessage}", 
                airport.IATA_Code, airport.Name, innerMessage);
            
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    return new ErrorResult($"Havalimani eklenirken hata: Bu IATA kodu zaten mevcut. SQL: {sqlEx.Message}");
                else if (sqlEx.Number == 515)
                    return new ErrorResult($"Havalimani eklenirken hata: Zorunlu alan eksik. SQL: {sqlEx.Message}");
            }
            
            return new ErrorResult($"Havalimani eklenirken veritabani hatasi: {innerMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding airport: {IATACode} - {Name}", airport.IATA_Code, airport.Name);
            return new ErrorResult($"Havalimani eklenirken hata olustu: {ex.Message}");
        }
    }

    //---Mevcut havalimanini guncelleyen metot---//
    public async Task<Result> UpdateAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(airport);

        await _unitOfWork.Airports.UpdateAsync(airport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove($"{CacheKeyPrefix}all");
        _cache.Remove($"{CacheKeyPrefix}id_{airport.Id}");
        _logger.LogDebug("Cache cleared after airport update: {AirportId}", airport.Id);
        return new SuccessResult("Havalimani guncellendi.");
    }

    //---Havalimanini soft delete eden metot---//
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Airports.SoftDeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove($"{CacheKeyPrefix}all");
        _cache.Remove($"{CacheKeyPrefix}id_{id}");
        _logger.LogDebug("Cache cleared after airport delete: {AirportId}", id);
        return new SuccessResult("Havalimani silindi.");
    }

    /// <summary>
    /// "iata/SAW" → "SAW", "IST" → "IST" gibi yanlis yazimlari duzeltir.
    /// </summary>
    private static string NormalizeIataCodeForLookup(string iataCode)
    {
        if (string.IsNullOrWhiteSpace(iataCode)) return iataCode ?? string.Empty;
        var s = iataCode.Trim();
        if (s.StartsWith("iata/", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(5).Trim();
        // Turkce I (U+0130) ve i (U+0131) → ASCII I
        return s.Replace("\u0130", "I").Replace("\u0131", "I");
    }
}

