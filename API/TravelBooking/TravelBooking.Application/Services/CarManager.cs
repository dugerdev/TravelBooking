using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace TravelBooking.Application.Services;

public class CarManager : ICarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<Car> _validator;
    private readonly ILogger<CarManager> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "car_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public CarManager(
        IUnitOfWork unitOfWork,
        IValidator<Car> validator,
        ILogger<CarManager> logger,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    public async Task<DataResult<Car>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting car by id: {CarId}", id);
        
        var cacheKey = $"{CacheKeyPrefix}id_{id}";
        if (_cache.TryGetValue(cacheKey, out Car? cachedCar) && cachedCar != null)
        {
            return new SuccessDataResult<Car>(cachedCar);
        }

        var car = await _unitOfWork.Cars.GetByIdAsync(id, cancellationToken);

        if (car is null)
            return new ErrorDataResult<Car>(null!, "Arac bulunamadi.");

        _cache.Set(cacheKey, car, CacheExpiration);
        return new SuccessDataResult<Car>(car);
    }

    public async Task<DataResult<IEnumerable<Car>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{CacheKeyPrefix}all";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Car>? cachedCars) && cachedCars != null)
        {
            return new SuccessDataResult<IEnumerable<Car>>(cachedCars);
        }

        var cars = await _unitOfWork.Cars.GetAllAsync(cancellationToken);
        _cache.Set(cacheKey, cars, CacheExpiration);
        
        return new SuccessDataResult<IEnumerable<Car>>(cars);
    }

    public async Task<DataResult<PagedResult<Car>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting cars with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Cars.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Car>>(pagedResult);
    }

    public async Task<DataResult<IEnumerable<Car>>> GetByLocationAsync(string location, CancellationToken cancellationToken = default)
    {
        var cars = await _unitOfWork.Cars.FindAsync(c => c.Location.Contains(location), cancellationToken);
        return new SuccessDataResult<IEnumerable<Car>>(cars);
    }

    public async Task<DataResult<IEnumerable<Car>>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var cars = await _unitOfWork.Cars.FindAsync(c => c.Category == category, cancellationToken);
        return new SuccessDataResult<IEnumerable<Car>>(cars);
    }

    public async Task<DataResult<IEnumerable<Car>>> SearchCarsAsync(string? location, string? category, decimal? maxPricePerDay, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.Set<Car>().Where(c => !c.IsDeleted && c.IsActive && c.IsAvailable);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(c => c.Location.Contains(location));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(c => c.Category == category);

        if (maxPricePerDay.HasValue)
            query = query.Where(c => c.PricePerDay.Amount <= maxPricePerDay.Value);

        var cars = await query.ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Car>>(cars);
    }

    public async Task<DataResult<PagedResult<Car>>> SearchCarsWithFiltersAsync(CarSearchFilterDto filters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching cars with filters");

        var query = _unitOfWork.Context.Set<Car>()
            .Where(c => !c.IsDeleted && c.IsActive && c.IsAvailable);

        if (!string.IsNullOrWhiteSpace(filters.Location))
            query = query.Where(c => c.Location.Contains(filters.Location));

        if (filters.MinPrice.HasValue)
            query = query.Where(c => c.PricePerDay.Amount >= filters.MinPrice.Value);
        if (filters.MaxPrice.HasValue)
            query = query.Where(c => c.PricePerDay.Amount <= filters.MaxPrice.Value);

        if (filters.Transmissions != null && filters.Transmissions.Any())
            query = query.Where(c => filters.Transmissions.Contains(c.Transmission));
        if (filters.FuelTypes != null && filters.FuelTypes.Any())
            query = query.Where(c => filters.FuelTypes.Contains(c.FuelType));
        if (filters.Categories != null && filters.Categories.Any())
            query = query.Where(c => filters.Categories.Contains(c.Category));
        if (filters.Brands != null && filters.Brands.Any())
            query = query.Where(c => filters.Brands.Contains(c.Brand));
        if (filters.Suppliers != null && filters.Suppliers.Any())
            query = query.Where(c => c.Supplier != null && filters.Suppliers.Contains(c.Supplier));

        if (filters.MinSeats.HasValue)
            query = query.Where(c => c.Seats >= filters.MinSeats.Value);
        if (filters.MinDoors.HasValue)
            query = query.Where(c => c.Doors >= filters.MinDoors.Value);

        if (filters.MileagePolicies != null && filters.MileagePolicies.Any())
            query = query.Where(c => filters.MileagePolicies.Contains(c.MileagePolicy));
        if (filters.FuelPolicies != null && filters.FuelPolicies.Any())
            query = query.Where(c => filters.FuelPolicies.Contains(c.FuelPolicy));
        if (filters.PickupLocationTypes != null && filters.PickupLocationTypes.Any())
            query = query.Where(c => filters.PickupLocationTypes.Contains(c.PickupLocationType));

        if (filters.HasAirConditioning == true)
            query = query.Where(c => c.HasAirConditioning);
        if (filters.HasGPS == true)
            query = query.Where(c => c.HasGPS);

        if (filters.MinRating.HasValue)
            query = query.Where(c => c.Rating >= filters.MinRating.Value);

        query = (filters.SortBy?.ToLowerInvariant()) switch
        {
            "price_asc" => query.OrderBy(c => c.PricePerDay.Amount),
            "price_desc" => query.OrderByDescending(c => c.PricePerDay.Amount),
            "rating_desc" => query.OrderByDescending(c => c.Rating),
            "rating_asc" => query.OrderBy(c => c.Rating),
            _ => query.OrderBy(c => c.PricePerDay.Amount)
        };

        var pageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1;
        var pageSize = filters.PageSize > 0 ? filters.PageSize : 20;
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<Car>(items, totalCount, pageNumber, pageSize);
        return new SuccessDataResult<PagedResult<Car>>(pagedResult);
    }

    public async Task<Result> AddAsync(Car car, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new car: {Brand} {Model}", car.Brand, car.Model);
        
        try
        {
            // Pre-validation: Money object kontrolu
            if (car.PricePerDay == null)
            {
                _logger.LogError("Car PricePerDay is null for {Brand} {Model}", car.Brand, car.Model);
                return new ErrorResult("Arac fiyat bilgisi eksik. PricePerDay null olamaz.");
            }
            
            _logger.LogDebug("Car PricePerDay: Amount={Amount}, Currency={Currency}", 
                car.PricePerDay.Amount, car.PricePerDay.Currency);
            
            await _validator.ValidateAndThrowAsync(car, cancellationToken);

            await _unitOfWork.Cars.AddAsync(car, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}all");
            _logger.LogInformation("Car added successfully: {CarId}", car.Id);
            
            return new SuccessResult("Arac eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            var fullMessage = dbEx.Message;
            
            _logger.LogError(dbEx, "Database error while adding car: {Brand} {Model}. Inner: {InnerMessage}. Full: {FullMessage}", 
                car.Brand, car.Model, innerMessage, fullMessage);
            
            // SQL Server hata kodlarina gore daha aciklayici mesajlar
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError("SQL Error Number: {ErrorNumber}, Line: {LineNumber}, Procedure: {Procedure}, Message: {Message}", 
                    sqlEx.Number, sqlEx.LineNumber, sqlEx.Procedure, sqlEx.Message);
                
                if (sqlEx.Number == 515) // Cannot insert NULL
                {
                    return new ErrorResult($"Arac eklenirken hata: Zorunlu bir alan eksik (NULL deger). SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 547) // Foreign key constraint
                {
                    return new ErrorResult($"Arac eklenirken hata: Iliskili bir kayit bulunamadi. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint
                {
                    return new ErrorResult($"Arac eklenirken hata: Bu arac zaten mevcut. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 208) // Invalid object name (table doesn't exist)
                {
                    return new ErrorResult($"Arac eklenirken hata: Veritabani tablosu bulunamadi. Migration'lari uygulayin: dotnet ef database update");
                }
            }
            
            return new ErrorResult($"Arac eklenirken veritabani hatasi olustu: {innerMessage}. Detay: {fullMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding car: {Brand} {Model}", car.Brand, car.Model);
            return new ErrorResult("Arac eklenirken bir hata olustu. Lutfen tekrar deneyin.");
        }
    }

    public async Task<Result> UpdateAsync(Car car, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(car, cancellationToken);

            await _unitOfWork.Cars.UpdateAsync(car, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}id_{car.Id}");
            _cache.Remove($"{CacheKeyPrefix}all");
            
            return new SuccessResult("Arac guncellendi.");
        }
        catch (ValidationException ex)
        {
            return new ErrorResult($"Validation hatasi: {string.Join(", ", ex.Errors.Select(e => e.ErrorMessage))}");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Arac guncellenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.Cars.SoftDeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}id_{id}");
            _cache.Remove($"{CacheKeyPrefix}all");
            
            return new SuccessResult("Arac silindi.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Arac silinirken hata olustu: {ex.Message}");
        }
    }
}
