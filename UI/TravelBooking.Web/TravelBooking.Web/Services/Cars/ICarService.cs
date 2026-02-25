using TravelBooking.Web.DTOs.Cars;
using TravelBooking.Web.DTOs.Common;

namespace TravelBooking.Web.Services.Cars;

public interface ICarService
{
    Task<(bool Success, string Message, CarDto? Car)> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message, List<CarDto> Cars)> GetAllAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, PagedResultDto<CarDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<(bool Success, string Message, List<CarDto> Cars)> SearchAsync(string? location, string? category, decimal? maxPricePerDay, CancellationToken ct = default);
    Task<(bool Success, string Message)> CreateAsync(CreateCarDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateCarDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default);
}
