using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace TravelBooking.Application.Services;

public class NewsManager : INewsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<NewsArticle> _validator;
    private readonly ILogger<NewsManager> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "news_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public NewsManager(
        IUnitOfWork unitOfWork,
        IValidator<NewsArticle> validator,
        ILogger<NewsManager> logger,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    public async Task<DataResult<NewsArticle>> GetByIdAsync(Guid id, bool incrementViewCount = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting news by id: {NewsId}, IncrementView: {IncrementView}", id, incrementViewCount);
        
        var news = await _unitOfWork.News.GetByIdAsync(id, cancellationToken);

        if (news is null)
            return new ErrorDataResult<NewsArticle>(null!, "Haber bulunamadi.");

        if (incrementViewCount)
        {
            news.IncrementViewCount();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _cache.Remove($"{CacheKeyPrefix}id_{id}");
        }

        return new SuccessDataResult<NewsArticle>(news);
    }

    public async Task<DataResult<IEnumerable<NewsArticle>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{CacheKeyPrefix}all";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<NewsArticle>? cachedNews) && cachedNews != null)
        {
            return new SuccessDataResult<IEnumerable<NewsArticle>>(cachedNews);
        }

        var news = await _unitOfWork.News.GetAllAsync(cancellationToken);
        _cache.Set(cacheKey, news, CacheExpiration);
        
        return new SuccessDataResult<IEnumerable<NewsArticle>>(news);
    }

    public async Task<DataResult<PagedResult<NewsArticle>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting news with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.News.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<NewsArticle>>(pagedResult);
    }

    public async Task<DataResult<IEnumerable<NewsArticle>>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var news = await _unitOfWork.News.FindAsync(n => n.Category == category && n.IsPublished, cancellationToken);
        return new SuccessDataResult<IEnumerable<NewsArticle>>(news);
    }

    public async Task<DataResult<IEnumerable<NewsArticle>>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{CacheKeyPrefix}published";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<NewsArticle>? cachedNews) && cachedNews != null)
        {
            return new SuccessDataResult<IEnumerable<NewsArticle>>(cachedNews);
        }

        var news = await _unitOfWork.News.FindAsync(n => n.IsPublished, cancellationToken);
        _cache.Set(cacheKey, news, CacheExpiration);
        
        return new SuccessDataResult<IEnumerable<NewsArticle>>(news);
    }

    public async Task<DataResult<IEnumerable<NewsArticle>>> SearchNewsAsync(string? query, string? category, CancellationToken cancellationToken = default)
    {
        var dbQuery = _unitOfWork.Context.Set<NewsArticle>().Where(n => !n.IsDeleted && n.IsPublished);

        if (!string.IsNullOrWhiteSpace(query))
            dbQuery = dbQuery.Where(n => n.Title.Contains(query) || n.Summary.Contains(query));

        if (!string.IsNullOrWhiteSpace(category))
            dbQuery = dbQuery.Where(n => n.Category == category);

        var news = await dbQuery.OrderByDescending(n => n.PublishDate).ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<NewsArticle>>(news);
    }

    public async Task<Result> AddAsync(NewsArticle news, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new news: {Title}", news.Title);
        
        try
        {
            await _validator.ValidateAndThrowAsync(news, cancellationToken);

            await _unitOfWork.News.AddAsync(news, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _cache.Remove($"{CacheKeyPrefix}all");
            _cache.Remove($"{CacheKeyPrefix}published");
            _logger.LogInformation("News added successfully: {NewsId}", news.Id);
            
            return new SuccessResult("Haber eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            var fullMessage = dbEx.Message;
            
            _logger.LogError(dbEx, "Database error while adding news: {Title}. Inner: {InnerMessage}. Full: {FullMessage}", 
                news.Title, innerMessage, fullMessage);
            
            // SQL Server hata kodlarina gore daha aciklayici mesajlar
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError("SQL Error Number: {ErrorNumber}, Line: {LineNumber}, Procedure: {Procedure}, Message: {Message}", 
                    sqlEx.Number, sqlEx.LineNumber, sqlEx.Procedure, sqlEx.Message);
                
                if (sqlEx.Number == 515) // Cannot insert NULL
                {
                    return new ErrorResult($"Haber eklenirken hata: Zorunlu bir alan eksik (NULL deger). SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 547) // Foreign key constraint
                {
                    return new ErrorResult($"Haber eklenirken hata: Iliskili bir kayit bulunamadi. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint
                {
                    return new ErrorResult($"Haber eklenirken hata: Bu haber zaten mevcut. SQL Hatasi: {sqlEx.Message}");
                }
                else if (sqlEx.Number == 208) // Invalid object name (table doesn't exist)
                {
                    return new ErrorResult($"Haber eklenirken hata: Veritabani tablosu bulunamadi. Migration'lari uygulayin: dotnet ef database update");
                }
            }
            
            return new ErrorResult($"Haber eklenirken veritabani hatasi olustu: {innerMessage}. Detay: {fullMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding news: {Title}", news.Title);
            return new ErrorResult($"Haber eklenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(NewsArticle news, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(news, cancellationToken);

        await _unitOfWork.News.UpdateAsync(news, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove($"{CacheKeyPrefix}id_{news.Id}");
        _cache.Remove($"{CacheKeyPrefix}all");
        _cache.Remove($"{CacheKeyPrefix}published");
        
        return new SuccessResult("Haber guncellendi.");
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.News.SoftDeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove($"{CacheKeyPrefix}id_{id}");
        _cache.Remove($"{CacheKeyPrefix}all");
        _cache.Remove($"{CacheKeyPrefix}published");
        
        return new SuccessResult("Haber silindi.");
    }
}
