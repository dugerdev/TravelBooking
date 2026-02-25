using TravelBooking.Web.DTOs.Admin;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.DTOs.Flights;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.DTOs.Airports;

namespace TravelBooking.Web.Services.Admin;

public interface IAdminService
{
    // Statistics
    Task<(bool Success, string Message, DashboardStatisticsDto? Data)> GetDashboardStatisticsAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, RevenueStatisticsDto? Data)> GetRevenueStatisticsAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
    Task<(bool Success, string Message, ReservationStatisticsDto? Data)> GetReservationStatisticsAsync(CancellationToken ct = default);
    
    // Users
    Task<(bool Success, string Message, PagedResultDto<UserDto>? Data)> GetUsersPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<(bool Success, string Message, UserDto? Data)> GetUserByIdAsync(string id, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> DeleteUserAsync(string id, CancellationToken ct = default);
    Task<(bool Success, string Message)> LockUserAsync(string id, DateTime? lockoutEnd, CancellationToken ct = default);
    Task<(bool Success, string Message)> UnlockUserAsync(string id, CancellationToken ct = default);
    
    // Flights
    Task<(bool Success, string Message, List<FlightDto> Data)> GetAllFlightsAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, PagedResultDto<FlightDto>? Data)> GetFlightsPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<(bool Success, string Message, FlightDto? Data)> GetFlightByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message)> CreateFlightAsync(CreateFlightDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateFlightAsync(Guid id, UpdateFlightDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> DeleteFlightAsync(Guid id, CancellationToken ct = default);
    
    // Airports
    Task<(bool Success, string Message, List<AirportDto> Data)> GetAllAirportsAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, AirportDto? Data)> GetAirportByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message)> CreateAirportAsync(CreateAirportDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateAirportAsync(Guid id, UpdateAirportDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> DeleteAirportAsync(Guid id, CancellationToken ct = default);
    
    // Reservations
    Task<(bool Success, string Message, List<ReservationDto> Data)> GetAllReservationsAsync(string? status, CancellationToken ct = default);
    Task<(bool Success, string Message, ReservationDto? Data)> GetReservationByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message)> CancelReservationAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message)> RefundReservationAsync(Guid id, CancellationToken ct = default);
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
}

public class UpdateUserDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class UpdateFlightDto
{
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
}
