using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace TravelBooking.Application.Services;

public class TourManager : ITourService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<Tour> _validator;
    private readonly ILogger<TourManager> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "tour_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(20);

    public TourManager(
        IUnitOfWork unitOfWork,
        IValidator<Tour> validator,
        ILogger<TourManager> logger,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    public async Task<DataResult<Tour>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tour by id: {TourId}", id);
        
        var cacheKey = $"{CacheKeyPrefix}id_{id}";
        if (_cache.TryGetValue(cacheKey, out Tour? cachedTour) && cachedTour != null)
        {
            return new SuccessDataResult<Tour>(cachedTour);
        }

        var tour = await _unitOfWork.Tours.GetByIdAsync(id, cancellationToken);

        if (tour is null)
            return new ErrorDataResult<Tour>(null!, "Tur bulunamadi.");

        _cache.Set(cacheKey, tour, CacheExpiration);
        return new SuccessDataResult<Tour>(tour);
    }

    public async Task<DataResult<IEnumerable<Tour>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{CacheKeyPrefix}all";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Tour>? cachedTours) && cachedTours != null)
        {
            return new SuccessDataResult<IEnumerable<Tour>>(cachedTours);
        }

        var tours = await _unitOfWork.Tours.GetAllAsync(cancellationToken);
        _cache.Set(cacheKey, tours, CacheExpiration);
        
        return new SuccessDataResult<IEnumerable<Tour>>(tours);
    }

    public async Task<DataResult<PagedResult<Tour>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tours with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Tours.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Tour>>(pagedResult);
    }

    public async Task<DataResult<IEnumerable<Tour>>> GetByDestinationAsync(string destination, CancellationToken cancellationToken = default)
    {
        var tours = await _unitOfWork.Tours.FindAsync(t => t.Destination.Contains(destination), cancellationToken);
        return new SuccessDataResult<IEnumerable<Tour>>(tours);
    }

    public async Task<DataResult<IEnumerable<Tour>>> SearchToursAsync(string? destination, int? minDuration, int? maxDuration, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.Set<Tour>().Where(t => !t.IsDeleted && t.IsActive);

        if (!string.IsNullOrWhiteSpace(destination))
            query = query.Where(t => t.Destination.Contains(destination));

        if (minDuration.HasValue)
            query = query.Where(t => t.Duration >= minDuration.Value);

        if (maxDuration.HasValue)
            query = query.Where(t => t.Duration <= maxDuration.Value);

        var tours = await query.ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Tour>>(tours);
    }

    public async Task<Result> AddAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new tour: {Name}", tour.Name);
        
        try
        {
            await _validator.ValidateAndThrowAsync(tour, cancellationToken);

            await _unitOfWork.Tours.AddAsync(tour, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}all");
            _logger.LogInformation("Tour added successfully: {TourId}", tour.Id);
            
            return new SuccessResult("Tur eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            var fullMessage = dbEx.Message;
            
            _logger.LogError(dbEx, "Database error while adding tour: {Name}. Inner: {InnerMessage}. Full: {FullMessage}", 
                tour.Name, innerMessage, fullMessage);
            
            // SQL Server hata kodlarina gore daha aciklayici mesajlar
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError("SQL Error Number: {ErrorNumber}, Line: {LineNumber}, Procedure: {Procedure}, Message: {Message}", 
                    sqlEx.Number, sqlEx.LineNumber, sqlEx.Procedure, sqlEx.Message);
                
                if (sqlEx.Number == 515) // Cannot insert NULL
                {
                    return new ErrorResult($"Tur eklenirken hata: Zorunlu bir alan eksik (NULL deger). SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 547) // Foreign key constraint
                {
                    return new ErrorResult($"Tur eklenirken hata: Iliskili bir kayit bulunamadi. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint
                {
                    return new ErrorResult($"Tur eklenirken hata: Bu tur zaten mevcut. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 208) // Invalid object name (table doesn't exist)
                {
                    return new ErrorResult($"Tur eklenirken hata: Veritabani tablosu bulunamadi. Migration'lari uygulayin: dotnet ef database update");
                }
            }
            
            return new ErrorResult($"Tur eklenirken veritabani hatasi olustu: {innerMessage}. Detay: {fullMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tour: {Name}", tour.Name);
            return new ErrorResult($"Tur eklenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(tour, cancellationToken);

        await _unitOfWork.Tours.UpdateAsync(tour, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove($"{CacheKeyPrefix}id_{tour.Id}");
        _cache.Remove($"{CacheKeyPrefix}all");
        
        return new SuccessResult("Tur guncellendi.");
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Tours.SoftDeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove($"{CacheKeyPrefix}id_{id}");
        _cache.Remove($"{CacheKeyPrefix}all");
        
        return new SuccessResult("Tur silindi.");
    }
}
