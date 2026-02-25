using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

/// <summary>
/// News servisi icin sozlesme (interface)
/// </summary>
public interface INewsService
{
    Task<DataResult<NewsArticle>> GetByIdAsync(Guid id, bool incrementViewCount = false, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<NewsArticle>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<NewsArticle>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<NewsArticle>>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<NewsArticle>>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<NewsArticle>>> SearchNewsAsync(string? query, string? category, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(NewsArticle news, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(NewsArticle news, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
