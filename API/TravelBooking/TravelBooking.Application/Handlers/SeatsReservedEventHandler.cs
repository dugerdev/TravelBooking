using TravelBooking.Domain.Events;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Handlers;

//---Koltuk rezerve edildiginde loglama yapan handler---//
public class SeatsReservedEventHandler : IDomainEventHandler<SeatsReservedEvent>
{
    private readonly ILogger<SeatsReservedEventHandler> _logger;

    public SeatsReservedEventHandler(ILogger<SeatsReservedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SeatsReservedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SeatsReservedEvent for flight: {FlightId}, Seats: {ReservedCount}", 
            domainEvent.FlightId, domainEvent.ReservedCount);
        
        //---Sadece loglama yapiyoruz---//
        return Task.CompletedTask;
    }
}
