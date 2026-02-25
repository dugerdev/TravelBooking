using TravelBooking.Web.DTOs.Tours;
using TravelBooking.Web.DTOs.Common;

namespace TravelBooking.Web.Services.Tours;

public interface ITourService
{
    Task<(bool Success, string Message, TourDto? Tour)> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message, List<TourDto> Tours)> GetAllAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, PagedResultDto<TourDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<(bool Success, string Message, List<TourDto> Tours)> SearchAsync(string? destination, int? minDuration, int? maxDuration, CancellationToken ct = default);
    Task<(bool Success, string Message)> CreateAsync(CreateTourDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateTourDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default);
}
