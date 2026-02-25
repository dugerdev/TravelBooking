using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Contracts;
using TravelBooking.Domain.Events;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Handlers;

//---Odeme basarisiz oldugunda email gonderen handler---//
public class PaymentFailedEventHandler : IDomainEventHandler<PaymentFailedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IReservationService _reservationService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<PaymentFailedEventHandler> _logger;

    public PaymentFailedEventHandler(
        IEmailService emailService,
        IReservationService reservationService,
        UserManager<AppUser> userManager,
        ILogger<PaymentFailedEventHandler> logger)
    {
        _emailService = emailService;
        _reservationService = reservationService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(PaymentFailedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling PaymentFailedEvent for reservation: {ReservationId}, PNR: {PNR}", 
            domainEvent.ReservationId, domainEvent.PNR);
        
        try
        {
            //---Rezervasyon bilgilerini al---//
            var reservationResult = await _reservationService.GetByIdAsync(domainEvent.ReservationId, cancellationToken);
            
            if (!reservationResult.Success || reservationResult.Data == null)
            {
                _logger.LogWarning("Reservation not found for PaymentFailedEvent: {ReservationId}", domainEvent.ReservationId);
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
            var emailBody = $@"
                <html>
                <body>
                    <h2>Odeme Basarisiz - Gocebe</h2>
                    <p>Sayin {user.UserName ?? "Kullanici"},</p>
                    <p>Rezervasyonunuz (PNR: {domainEvent.PNR}) icin odeme islemi basarisiz oldu.</p>
                    <p>Lutfen odeme bilgilerinizi kontrol ederek tekrar deneyiniz.</p>
                    <p>Iyi gunler dileriz.</p>
                </body>
                </html>";

            await _emailService.SendEmailAsync(
                user.Email,
                "Odeme Basarisiz - Gocebe",
                emailBody,
                true,
                cancellationToken);

            _logger.LogInformation("Payment failed email sent successfully: ReservationId {ReservationId}, Email {Email}", 
                domainEvent.ReservationId, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PaymentFailedEvent for reservation: {ReservationId}", domainEvent.ReservationId);
            //---Email gonderme hatasi islemi durdurmamali---//
        }
    }
}
