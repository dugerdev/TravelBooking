using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Reservations;

public class ReservationService : IReservationService
{
    private readonly ITravelBookingApiClient _api;

    public ReservationService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, Guid? ReservationId)> CreateAsync(CreateReservationDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<string>(ApiEndpoints.Reservations, dto, ct);
        if (res == null)
            return (false, "Rezervasyon olusturulamadi.", null);
        if (!res.Success)
            return (false, res.Message ?? "Rezervasyon olusturulamadi.", null);
        Guid? id = null;
        if (!string.IsNullOrEmpty(res.Data) && Guid.TryParse(res.Data, out var g))
            id = g;
        return (true, res.Message ?? "Rezervasyon olusturuldu.", id);
    }

    public async Task<(bool Success, string Message, ReservationDto? Reservation)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<ReservationDto>(ApiEndpoints.ReservationById(id), ct);
        if (res == null)
            return (false, "Reservation not found.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, ReservationDto? Reservation)> GetByPNRAsync(string pnr, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<ReservationDto>(ApiEndpoints.ReservationByPnr(pnr), ct);
        if (res == null)
            return (false, "Reservation not found.", null);
        if (!res.Success)
            return (false, res.Message ?? "Reservation not found.", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<ReservationDto> Reservations)> GetMyReservationsAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<List<ReservationDto>>(ApiEndpoints.ReservationsList, ct);
        if (res == null)
            return (false, "Rezervasyonlar alinamadi.", new List<ReservationDto>());
        if (!res.Success)
            return (false, res.Message ?? "", new List<ReservationDto>());
        var list = res.Data?.ToList() ?? new List<ReservationDto>();
        return (true, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, PagedResultDto<ReservationDto>? Paged)> GetMyReservationsPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var path = $"api/account/reservations?PageNumber={pageNumber}&PageSize={pageSize}";
        var res = await _api.GetAsync<PagedResultDto<ReservationDto>>(path, ct);
        if (res == null)
            return (false, "Rezervasyonlar alinamadi.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.PostNoBodyAsync<object>(ApiEndpoints.ReservationCancel(id), ct);
        if (res == null)
            return (false, "Cancellation could not be performed.");
        return res.Success ? (true, res.Message ?? "Rezervasyon iptal edildi.") : (false, res.Message ?? "Iptal basarisiz.");
    }
}
