using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Admin;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.DTOs.Flights;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.DTOs.Airports;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Admin;

public class AdminService : IAdminService
{
    private readonly ITravelBookingApiClient _api;

    public AdminService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    // Statistics
    public async Task<(bool Success, string Message, DashboardStatisticsDto? Data)> GetDashboardStatisticsAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<DashboardStatisticsDto>(ApiEndpoints.AdminStatisticsDashboard, ct);
        if (res == null)
            return (false, "Could not retrieve statistics.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, RevenueStatisticsDto? Data)> GetRevenueStatisticsAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        var qs = "";
        if (startDate.HasValue) qs += "&startDate=" + startDate.Value.ToString("O");
        if (endDate.HasValue) qs += "&endDate=" + endDate.Value.ToString("O");
        var res = await _api.GetAsync<RevenueStatisticsDto>(ApiEndpoints.AdminStatisticsRevenue(qs.TrimStart('&')), ct);
        if (res == null)
            return (false, "Could not retrieve revenue statistics.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, ReservationStatisticsDto? Data)> GetReservationStatisticsAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<ReservationStatisticsDto>(ApiEndpoints.AdminStatisticsReservations, ct);
        if (res == null)
            return (false, "Could not retrieve reservation statistics.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    // Users
    public async Task<(bool Success, string Message, PagedResultDto<UserDto>? Data)> GetUsersPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var path = $"api/admin/users?PageNumber={pageNumber}&PageSize={pageSize}";
        var res = await _api.GetAsync<PagedResultDto<UserDto>>(path, ct);
        if (res == null)
            return (false, "Could not retrieve users.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, UserDto? Data)> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<UserDto>(ApiEndpoints.AdminUserById(id), ct);
        if (res == null)
            return (false, "User not found.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<UserDto>($"api/admin/users/{id}", dto, ct);
        if (res == null)
            return (false, "Could not update user.");
        return res.Success ? (true, res.Message ?? "User updated.") : (false, res.Message ?? "Update failed.");
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(string id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<UserDto>(ApiEndpoints.AdminUserById(id), ct);
        if (res == null)
            return (false, "Could not delete user.");
        return res.Success ? (true, res.Message ?? "User deleted.") : (false, res.Message ?? "Deletion failed.");
    }

    public async Task<(bool Success, string Message)> LockUserAsync(string id, DateTime? lockoutEnd, CancellationToken ct = default)
    {
        var body = new { LockoutEnd = lockoutEnd };
        var res = await _api.PostAsync<UserDto>($"api/admin/users/{id}/lock", body, ct);
        if (res == null)
            return (false, "Could not lock user.");
        return res.Success ? (true, res.Message ?? "User locked.") : (false, res.Message ?? "Lock failed.");
    }

    public async Task<(bool Success, string Message)> UnlockUserAsync(string id, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<UserDto>(ApiEndpoints.AdminUserUnlock(id), null, ct);
        if (res == null)
            return (false, "Could not unlock user.");
        return res.Success ? (true, res.Message ?? "User unlocked.") : (false, res.Message ?? "Unlock failed.");
    }

    // Flights
    public async Task<(bool Success, string Message, List<FlightDto> Data)> GetAllFlightsAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<List<FlightDto>>(ApiEndpoints.AdminFlights, ct);
        if (res == null)
            return (false, "Could not retrieve flights.", new List<FlightDto>());
        if (!res.Success)
            return (false, res.Message ?? "", new List<FlightDto>());
        return (true, res.Message ?? "", res.Data ?? new List<FlightDto>());
    }

    public async Task<(bool Success, string Message, PagedResultDto<FlightDto>? Data)> GetFlightsPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var path = ApiEndpoints.AdminFlightsPaged(pageNumber, pageSize);
        var res = await _api.GetAsync<PagedResultDto<FlightDto>>(path, ct);
        if (res == null)
            return (false, "Could not retrieve flights.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, FlightDto? Data)> GetFlightByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<FlightDto>($"api/admin/flights/{id}", ct);
        if (res == null)
            return (false, "Flight not found.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CreateFlightAsync(CreateFlightDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<FlightDto>(ApiEndpoints.AdminFlightsCreate, dto, ct);
        if (res == null)
            return (false, "Could not create flight.");
        return res.Success ? (true, res.Message ?? "Flight created.") : (false, res.Message ?? "Creation failed.");
    }

    public async Task<(bool Success, string Message)> UpdateFlightAsync(Guid id, UpdateFlightDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<FlightDto>(ApiEndpoints.AdminFlightById(id), dto, ct);
        if (res == null)
            return (false, "Could not update flight.");
        return res.Success ? (true, res.Message ?? "Flight updated.") : (false, res.Message ?? "Update failed.");
    }

    public async Task<(bool Success, string Message)> DeleteFlightAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<FlightDto>($"api/admin/flights/{id}", ct);
        if (res == null)
            return (false, "Could not delete flight.");
        return res.Success ? (true, res.Message ?? "Flight deleted.") : (false, res.Message ?? "Deletion failed.");
    }

    public async Task<(bool Success, string Message, List<AirportDto> Data)> GetAllAirportsAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<IEnumerable<AirportDto>>(ApiEndpoints.AdminAirports, ct);
        if (res == null)
            return (false, "Could not retrieve airports.", new List<AirportDto>());
        if (!res.Success)
            return (false, res.Message ?? "", new List<AirportDto>());
        return (true, res.Message ?? "", res.Data?.ToList() ?? new List<AirportDto>());
    }

    public async Task<(bool Success, string Message, AirportDto? Data)> GetAirportByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<AirportDto>(ApiEndpoints.AdminAirportById(id), ct);
        if (res == null)
            return (false, "Airport not found.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CreateAirportAsync(CreateAirportDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<AirportDto>(ApiEndpoints.AdminAirports, dto, ct);
        if (res == null)
            return (false, "Could not add airport.");
        return res.Success ? (true, res.Message ?? "Airport added.") : (false, res.Message ?? "Addition failed.");
    }

    public async Task<(bool Success, string Message)> UpdateAirportAsync(Guid id, UpdateAirportDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<AirportDto>(ApiEndpoints.AdminAirportById(id), dto, ct);
        if (res == null)
            return (false, "Could not update airport.");
        return res.Success ? (true, res.Message ?? "Airport updated.") : (false, res.Message ?? "Update failed.");
    }

    public async Task<(bool Success, string Message)> DeleteAirportAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<AirportDto>($"api/airports/{id}", ct);
        if (res == null)
            return (false, "Could not delete airport.");
        return res.Success ? (true, res.Message ?? "Airport deleted.") : (false, res.Message ?? "Deletion failed.");
    }

    // Reservations
    public async Task<(bool Success, string Message, List<ReservationDto> Data)> GetAllReservationsAsync(string? status, CancellationToken ct = default)
    {
        var path = ApiEndpoints.AdminReservations(status);
        var res = await _api.GetAsync<List<ReservationDto>>(path, ct);
        if (res == null)
            return (false, "Could not retrieve reservations.", new List<ReservationDto>());
        if (!res.Success)
            return (false, res.Message ?? "", new List<ReservationDto>());
        return (true, res.Message ?? "", res.Data ?? new List<ReservationDto>());
    }

    public async Task<(bool Success, string Message, ReservationDto? Data)> GetReservationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<ReservationDto>($"api/admin/reservations/{id}", ct);
        if (res == null)
            return (false, "Reservation not found.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CancelReservationAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<ReservationDto>(ApiEndpoints.AdminReservationCancel(id), null, ct);
        if (res == null)
            return (false, "Could not cancel reservation.");
        return res.Success ? (true, res.Message ?? "Reservation cancelled.") : (false, res.Message ?? "Cancellation failed.");
    }

    public async Task<(bool Success, string Message)> RefundReservationAsync(Guid id, CancellationToken ct = default)
    {
        // Cancel reservation first (which may trigger refund logic in the backend)
        var cancelRes = await CancelReservationAsync(id, ct);
        if (!cancelRes.Success)
            return cancelRes;
        
        // In a real implementation, this would call a payment gateway refund API
        // For now, we just mark it as refunded via cancellation
        return (true, "Refund process initiated. Will be processed automatically after payment provider integration.");
    }
}
