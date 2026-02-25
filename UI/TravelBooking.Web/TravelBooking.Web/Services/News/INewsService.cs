using TravelBooking.Web.DTOs.News;
using TravelBooking.Web.DTOs.Common;

namespace TravelBooking.Web.Services.News;

public interface INewsService
{
    Task<(bool Success, string Message, NewsDto? News)> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message, List<NewsDto> News)> GetAllAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, PagedResultDto<NewsDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<(bool Success, string Message, List<NewsDto> News)> GetPublishedAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, List<NewsDto> News)> SearchAsync(string? query, string? category, CancellationToken ct = default);
    Task<(bool Success, string Message)> CreateAsync(CreateNewsDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateNewsDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default);
}
