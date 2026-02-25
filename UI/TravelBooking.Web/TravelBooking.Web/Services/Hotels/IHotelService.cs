using TravelBooking.Web.DTOs.Hotels;
using TravelBooking.Web.DTOs.Common;

namespace TravelBooking.Web.Services.Hotels;

public interface IHotelService
{
    Task<(bool Success, string Message, HotelDto? Hotel)> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message, List<HotelDto> Hotels)> GetAllAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, PagedResultDto<HotelDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<(bool Success, string Message, List<HotelDto> Hotels)> SearchAsync(string? city, int? minStarRating, decimal? maxPricePerNight, CancellationToken ct = default);
    Task<(bool Success, string Message)> CreateAsync(CreateHotelDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateHotelDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default);
}
