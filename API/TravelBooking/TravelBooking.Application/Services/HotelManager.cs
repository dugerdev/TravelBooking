using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace TravelBooking.Application.Services;

public class HotelManager : IHotelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<Hotel> _validator;
    private readonly ILogger<HotelManager> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "hotel_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public HotelManager(
        IUnitOfWork unitOfWork,
        IValidator<Hotel> validator,
        ILogger<HotelManager> logger,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    public async Task<DataResult<Hotel>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting hotel by id: {HotelId}", id);
        
        var cacheKey = $"{CacheKeyPrefix}id_{id}";
        if (_cache.TryGetValue(cacheKey, out Hotel? cachedHotel) && cachedHotel != null)
        {
            return new SuccessDataResult<Hotel>(cachedHotel);
        }

        var hotel = await _unitOfWork.Hotels.GetByIdWithIncludesAsync(id, cancellationToken, h => h.Rooms);

        if (hotel is null)
            return new ErrorDataResult<Hotel>(null!, "Otel bulunamadi.");

        _cache.Set(cacheKey, hotel, CacheExpiration);
        return new SuccessDataResult<Hotel>(hotel);
    }

    public async Task<DataResult<IEnumerable<Hotel>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{CacheKeyPrefix}all";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Hotel>? cachedHotels) && cachedHotels != null)
        {
            return new SuccessDataResult<IEnumerable<Hotel>>(cachedHotels);
        }

        // Listing sayfasinda standart oda fiyati gosterildigi icin Rooms'u include et
        var hotels = await _unitOfWork.Context.Set<Hotel>()
            .Include(h => h.Rooms)
            .Where(h => !h.IsDeleted)
            .ToListAsync(cancellationToken);
        
        _cache.Set(cacheKey, hotels, CacheExpiration);
        
        return new SuccessDataResult<IEnumerable<Hotel>>(hotels);
    }

    public async Task<DataResult<PagedResult<Hotel>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting hotels with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Hotels.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Hotel>>(pagedResult);
    }

    public async Task<DataResult<IEnumerable<Hotel>>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        var hotels = await _unitOfWork.Hotels.FindAsync(h => h.City == city, cancellationToken);
        return new SuccessDataResult<IEnumerable<Hotel>>(hotels);
    }

    public async Task<DataResult<IEnumerable<Hotel>>> SearchHotelsAsync(string? city, int? minStarRating, decimal? maxPricePerNight, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.Set<Hotel>()
            .Include(h => h.Rooms)
            .Where(h => !h.IsDeleted && h.IsActive);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(h => h.City.Contains(city));

        if (minStarRating.HasValue)
            query = query.Where(h => h.StarRating >= minStarRating.Value);

        if (maxPricePerNight.HasValue)
            query = query.Where(h => h.PricePerNight.Amount <= maxPricePerNight.Value);

        var hotels = await query.ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Hotel>>(hotels);
    }

    public async Task<DataResult<PagedResult<Hotel>>> SearchHotelsWithFiltersAsync(Dtos.HotelSearchFilterDto filters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching hotels with filters");
        
        var query = _unitOfWork.Context.Set<Hotel>()
            .Include(h => h.Rooms)
            .Where(h => !h.IsDeleted && h.IsActive);

        // Location filters
        if (!string.IsNullOrWhiteSpace(filters.City))
            query = query.Where(h => h.City.Contains(filters.City));
        
        if (!string.IsNullOrWhiteSpace(filters.Country))
            query = query.Where(h => h.Country.Contains(filters.Country));

        // Price range
        if (filters.MinPrice.HasValue)
            query = query.Where(h => h.PricePerNight.Amount >= filters.MinPrice.Value);
        
        if (filters.MaxPrice.HasValue)
            query = query.Where(h => h.PricePerNight.Amount <= filters.MaxPrice.Value);

        // Star rating
        if (filters.MinStarRating.HasValue)
            query = query.Where(h => h.StarRating >= filters.MinStarRating.Value);
        
        if (filters.StarRatings != null && filters.StarRatings.Any())
            query = query.Where(h => filters.StarRatings.Contains(h.StarRating));

        // Property type
        if (filters.PropertyTypes != null && filters.PropertyTypes.Any())
            query = query.Where(h => filters.PropertyTypes.Contains(h.PropertyType));

        // Distance from center
        if (filters.MaxDistanceFromCenter.HasValue)
            query = query.Where(h => h.DistanceFromCenter <= filters.MaxDistanceFromCenter.Value);

        // Review score
        if (filters.MinReviewScore.HasValue)
            query = query.Where(h => h.Rating >= filters.MinReviewScore.Value);

        // Sustainability
        if (filters.MinSustainabilityLevel.HasValue)
            query = query.Where(h => h.SustainabilityLevel >= filters.MinSustainabilityLevel.Value);

        // Brand/Chain
        if (filters.Brands != null && filters.Brands.Any())
            query = query.Where(h => h.Brand != null && filters.Brands.Contains(h.Brand));

        // Neighbourhood
        if (filters.Neighbourhoods != null && filters.Neighbourhoods.Any())
            query = query.Where(h => h.Neighbourhood != null && filters.Neighbourhoods.Contains(h.Neighbourhood));

        // Facilities
        if (filters.HasFreeWifi == true)
            query = query.Where(h => h.HasFreeWifi);
        
        if (filters.HasParking == true)
            query = query.Where(h => h.HasParking);
        
        if (filters.HasPool == true)
            query = query.Where(h => h.HasPool);
        
        if (filters.HasRestaurant == true)
            query = query.Where(h => h.HasRestaurant);
        
        if (filters.HasAirConditioning == true)
            query = query.Where(h => h.HasAirConditioning);
        
        if (filters.HasFitnessCenter == true)
            query = query.Where(h => h.HasFitnessCenter);
        
        if (filters.HasSpa == true)
            query = query.Where(h => h.HasSpa);

        // Meal options
        if (filters.HasBreakfast == true)
            query = query.Where(h => h.HasBreakfast);

        // Booking options
        if (filters.HasFreeCancellation == true)
            query = query.Where(h => h.HasFreeCancellation);
        
        if (filters.NoPrepaymentNeeded == true)
            query = query.Where(h => h.NoPrepaymentNeeded);

        // Accessibility
        if (filters.HasAccessibilityFeatures == true)
            query = query.Where(h => h.HasAccessibilityFeatures);

        // Sorting
        query = filters.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(h => h.PricePerNight.Amount),
            "price_desc" => query.OrderByDescending(h => h.PricePerNight.Amount),
            "rating_desc" => query.OrderByDescending(h => h.Rating),
            "distance_asc" => query.OrderBy(h => h.DistanceFromCenter),
            "stars_desc" => query.OrderByDescending(h => h.StarRating),
            _ => query.OrderByDescending(h => h.Rating) // Default sort by rating
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Pagination
        var hotels = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<Hotel>
        {
            Items = hotels,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };

        return new SuccessDataResult<PagedResult<Hotel>>(result, $"{totalCount} hotels found");
    }

    public async Task<Result> AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new hotel: {Name}", hotel.Name);
        
        try
        {
            await _validator.ValidateAndThrowAsync(hotel, cancellationToken);

            await _unitOfWork.Hotels.AddAsync(hotel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}all");
            _logger.LogInformation("Hotel added successfully: {HotelId}", hotel.Id);
            
            return new SuccessResult("Otel eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            var fullMessage = dbEx.Message;
            
            _logger.LogError(dbEx, "Database error while adding hotel: {Name}. Inner: {InnerMessage}. Full: {FullMessage}", 
                hotel.Name, innerMessage, fullMessage);
            
            // SQL Server hata kodlarina gore daha aciklayici mesajlar
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError("SQL Error Number: {ErrorNumber}, Line: {LineNumber}, Procedure: {Procedure}, Message: {Message}", 
                    sqlEx.Number, sqlEx.LineNumber, sqlEx.Procedure, sqlEx.Message);
                
                if (sqlEx.Number == 515) // Cannot insert NULL
                {
                    return new ErrorResult($"Otel eklenirken hata: Zorunlu bir alan eksik (NULL deger). SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 547) // Foreign key constraint
                {
                    return new ErrorResult($"Otel eklenirken hata: Iliskili bir kayit bulunamadi. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint
                {
                    return new ErrorResult($"Otel eklenirken hata: Bu otel zaten mevcut. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 208) // Invalid object name (table doesn't exist)
                {
                    return new ErrorResult($"Otel eklenirken hata: Veritabani tablosu bulunamadi. Migration'lari uygulayin: dotnet ef database update");
                }
            }
            
            return new ErrorResult($"Otel eklenirken veritabani hatasi olustu: {innerMessage}. Detay: {fullMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding hotel: {Name}", hotel.Name);
            return new ErrorResult($"Otel eklenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(hotel, cancellationToken);

            await _unitOfWork.Hotels.UpdateAsync(hotel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}id_{hotel.Id}");
            _cache.Remove($"{CacheKeyPrefix}all");
            
            return new SuccessResult("Otel guncellendi.");
        }
        catch (ValidationException ex)
        {
            return new ErrorResult($"Validation hatasi: {string.Join(", ", ex.Errors.Select(e => e.ErrorMessage))}");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Otel guncellenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.Hotels.SoftDeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}id_{id}");
            _cache.Remove($"{CacheKeyPrefix}all");
            
            return new SuccessResult("Otel silindi.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Otel silinirken hata olustu: {ex.Message}");
        }
    }

    public void ClearCache()
    {
        // Tum otel cache'lerini temizle
        _logger.LogInformation("Otel cache'i temizleniyor...");
        // MemoryCache'de tum hotel_ ile baslayan key'leri temizlemek icin
        // basit bir yontem: cache'i yeniden olusturmak yerine, bilinen key'leri temizle
        // Gercek uygulamada tum key'leri takip etmek gerekir
        _cache.Remove($"{CacheKeyPrefix}all");
        _logger.LogInformation("Otel cache'i temizlendi.");
    }
}
