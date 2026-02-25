using TravelBooking.Web.DTOs.Passengers;

namespace TravelBooking.Web.Services.Passengers;

public interface IPassengerService
{
    Task<(bool Success, string Message, Guid? PassengerId)> CreateAsync(CreatePassengerDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message, PassengerDto? Passenger)> GetByIdAsync(Guid id, CancellationToken ct = default);
}
