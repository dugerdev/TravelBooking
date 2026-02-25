using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Events;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Handlers;

//---Bilet iptal edildiginde ucustan koltuklari serbest birakan handler---//
public class TicketCancelledEventHandler : IDomainEventHandler<TicketCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketCancelledEventHandler> _logger;

    public TicketCancelledEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<TicketCancelledEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TicketCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling TicketCancelledEvent for ticket: {TicketId}, Flight: {FlightId}", 
            domainEvent.TicketId, domainEvent.FlightId);
        
        try
        {
            //---Ucusu getir ve koltuklari serbest birak---//
            var flight = await _unitOfWork.Flights.GetByIdAsync(domainEvent.FlightId, cancellationToken);
            if (flight != null)
            {
                flight.ReleaseSeats(1);
                await _unitOfWork.Flights.UpdateAsync(flight, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Seat released for flight: {FlightId} after ticket cancellation: {TicketId}", 
                    domainEvent.FlightId, domainEvent.TicketId);
            }
            else
            {
                _logger.LogWarning("Flight not found for TicketCancelledEvent: {FlightId}", domainEvent.FlightId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling TicketCancelledEvent for ticket: {TicketId}", domainEvent.TicketId);
        }
    }
}
