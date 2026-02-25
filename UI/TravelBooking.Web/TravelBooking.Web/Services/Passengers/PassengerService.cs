using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Passengers;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Passengers;

public class PassengerService : IPassengerService
{
    private readonly ITravelBookingApiClient _api;

    public PassengerService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, Guid? PassengerId)> CreateAsync(CreatePassengerDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<string>(ApiEndpoints.Passengers, dto, ct);
        if (res == null)
            return (false, "Yolcu olusturulamadi.", null);
        if (!res.Success)
            return (false, res.Message ?? "Yolcu olusturulamadi.", null);
        
        Guid? id = null;
        if (!string.IsNullOrEmpty(res.Data) && Guid.TryParse(res.Data, out var g))
            id = g;
        
        return (true, res.Message ?? "Yolcu olusturuldu.", id);
    }

    public async Task<(bool Success, string Message, PassengerDto? Passenger)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<PassengerDto>(ApiEndpoints.PassengerById(id), ct);
        if (res == null)
            return (false, "Passenger not found.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }
}
