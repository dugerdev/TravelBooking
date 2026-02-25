using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Contracts;
using TravelBooking.Domain.Events;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Handlers;

//---Rezervasyon onaylandiginda email gonderen handler---//
public class ReservationConfirmedEventHandler : IDomainEventHandler<ReservationConfirmedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IReservationService _reservationService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ReservationConfirmedEventHandler> _logger;

    public ReservationConfirmedEventHandler(
        IEmailService emailService,
        IReservationService reservationService,
        UserManager<AppUser> userManager,
        ILogger<ReservationConfirmedEventHandler> logger)
    {
        _emailService = emailService;
        _reservationService = reservationService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(ReservationConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ReservationConfirmedEvent for reservation: {ReservationId}", domainEvent.ReservationId);
        
        try
        {
            //---Rezervasyon bilgilerini al---//
            var reservationResult = await _reservationService.GetByIdAsync(domainEvent.ReservationId, cancellationToken);
            
            if (!reservationResult.Success || reservationResult.Data == null)
            {
                _logger.LogWarning("Reservation not found for ReservationConfirmedEvent: {ReservationId}", domainEvent.ReservationId);
                return;
            }

            var reservation = reservationResult.Data;

            //---Kullanici bilgilerini al---//
            var user = await _userManager.FindByIdAsync(reservation.AppUserId);
            
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning("User not found or email is empty for reservation: {ReservationId}, UserId: {UserId}", 
                    domainEvent.ReservationId, reservation.AppUserId);
                return;
            }

            //---Email gonder---//
            await _emailService.SendReservationConfirmationAsync(
                user.Email,
                user.UserName ?? "Kullanici",
                reservation.PNR,
                reservation.TotalPrice,
                cancellationToken);

            _logger.LogInformation("Reservation confirmation email sent successfully: ReservationId {ReservationId}, Email {Email}", 
                domainEvent.ReservationId, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ReservationConfirmedEvent for reservation: {ReservationId}", domainEvent.ReservationId);
            //---Email gonderme hatasi islemi durdurmamali---//
        }
    }
}
