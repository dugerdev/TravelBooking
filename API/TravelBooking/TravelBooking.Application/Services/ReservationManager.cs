using System.Collections.Generic;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Services;
using TravelBooking.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

/// <summary>
/// Rezervasyonlara iliskin is kurallarini yoneten servis.
/// Rezervasyon olusturma, guncelleme, iptal etme ve sorgulama islemlerini yonetir.
/// </summary>
public class ReservationManager : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;                                        //---Tum repository'leri yoneten yapi---//
    private readonly IValidator<Reservation> _validator;                             //---Rezervasyon dogrulama kurallari---//
    private readonly ILogger<ReservationManager> _logger;                            //---Logging servisi---//
    private readonly ITicketService _ticketService;                                  //---Bilet servisi---//
    private readonly IPaymentService _paymentService;                                //---Odeme servisi---//
    private readonly IPricingPolicy _pricingPolicy;                                  //---Otomatik fiyatlandirma---//

    /// <summary>
    /// ReservationManager constructor.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work instance.</param>
    /// <param name="validator">Rezervasyon validator.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="ticketService">Bilet servisi.</param>
    /// <param name="paymentService">Odeme servisi.</param>
    /// <param name="pricingPolicy">Fiyatlandirma politikasi.</param>
    public ReservationManager(
        IUnitOfWork unitOfWork, 
        IValidator<Reservation> validator, 
        ILogger<ReservationManager> logger,
        ITicketService ticketService,
        IPaymentService paymentService,
        IPricingPolicy pricingPolicy)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _ticketService = ticketService;
        _paymentService = paymentService;
        _pricingPolicy = pricingPolicy;
    }

    /// <summary>
    /// ID'ye gore rezervasyon getirir. Tickets ve Flight navigation property'lerini de yukler.
    /// </summary>
    /// <param name="id">Rezervasyon ID'si.</param>
    /// <param name="cancellationToken">Iptal token'i.</param>
    /// <returns>Rezervasyon bilgileri veya hata mesaji.</returns>
    public async Task<DataResult<Reservation>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ThenInclude destegi icin IUnitOfWork uzerinden DbContext kullaniyoruz
        // AppUser, Passenger ve Airports include edilerek UserName ve ReservationSummary map edilebilsin
        // AsSplitQuery: birden fazla Include(Tickets) dali oldugunda her iki airport'un da yuklenmesini garanti eder
        var reservation = await _unitOfWork.Context.Set<Reservation>()
            .Where(r => r.Id == id && !r.IsDeleted)
            .Include(r => r.AppUser)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Passenger)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Flight)
                    .ThenInclude(f => f.DepartureAirport)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
            .Include(r => r.Passengers)
            .Include(r => r.Payments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (reservation is null)
            return new ErrorDataResult<Reservation>(null!, "Rezervasyon bulunamadi.");

        return new SuccessDataResult<Reservation>(reservation);
    }

    //---PNR'ye gore rezervasyon getiren metot---//
    public async Task<DataResult<Reservation>> GetByPNRAsync(string pnr, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Reservations.GetQueryable()
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Passenger)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Flight)
            .Include(r => r.Passengers)
            .Include(r => r.Payments)
            .Where(r => r.PNR == pnr);

        var reservation = await query.FirstOrDefaultAsync(cancellationToken);

        if (reservation is null)
            return new ErrorDataResult<Reservation>(null!, "Rezervasyon bulunamadi.");

        return new SuccessDataResult<Reservation>(reservation);
    }

    //---Kullanici ID'sine gore rezervasyonlari getiren metot---//
    public async Task<DataResult<IEnumerable<Reservation>>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Reservations.GetQueryable()
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Passenger)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Flight)
            .Include(r => r.Passengers)
            .Include(r => r.Payments)
            .Where(r => r.AppUserId == userId);

        var reservations = await query.ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Reservation>>(reservations);
    }

    //---Tum rezervasyonlari getiren metot---//
    public async Task<DataResult<IEnumerable<Reservation>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Include navigation properties to prevent N+1 queries
        var reservations = await _unitOfWork.Context.Set<Reservation>()
            .Include(r => r.AppUser)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Passenger)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Flight)
            .Include(r => r.Passengers)
            .Include(r => r.Payments)
            .Where(r => !r.IsDeleted)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Reservation>>(reservations);
    }

    //---Admin listesi: AppUser ve Tickets/Flight ile tum rezervasyonlar---//
    public async Task<DataResult<IEnumerable<Reservation>>> GetAllForAdminAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await _unitOfWork.Context.Set<Reservation>()
            .Where(r => !r.IsDeleted)
            .Include(r => r.AppUser)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Passenger)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Flight)
                    .ThenInclude(f => f.DepartureAirport)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Reservation>>(reservations);
    }

    //---Tum rezervasyonlari pagination ile getiren metot---//
    public async Task<DataResult<PagedResult<Reservation>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting reservations with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Reservations.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Reservation>>(pagedResult);
    }

    //---Kullanici ID'sine gore rezervasyonlari pagination ile getiren metot---//
    //---Rezervasyonlar tarih/saate gore siralanir (yeni → eski)---//
    public async Task<DataResult<PagedResult<Reservation>>> GetByUserIdPagedAsync(string userId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting reservations for user {UserId} with pagination: Page {PageNumber}, Size {PageSize}", userId, request.PageNumber, request.PageSize);
        
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var query = _unitOfWork.Context.Set<Reservation>()
            .Where(r => r.AppUserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.CreatedDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<Reservation>(items, totalCount, pageNumber, pageSize);
        return new SuccessDataResult<PagedResult<Reservation>>(pagedResult);
    }

    //---Yeni rezervasyon ekleyen metot---//
    //---Transaction yonetimi ile (SqlServerRetryingExecutionStrategy ile uyumlu degil; strategy kullanilmiyor)---//
    public async Task<Result> AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new reservation: PNR {PNR}, UserId {UserId}", reservation.PNR, reservation.AppUserId);
        
        await _validator.ValidateAndThrowAsync(reservation);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Reservation added successfully: {ReservationId} - PNR {PNR}", reservation.Id, reservation.PNR);
            return new SuccessResult("Rezervasyon eklendi.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error adding reservation: PNR {PNR}", reservation.PNR);
            return new ErrorResult($"Rezervasyon eklenirken hata olustu: {ex.Message}");
        }
    }

    //---Rezervasyon, bilet ve odeme ile birlikte olusturan metot---//
    //---Fiyatlar IPricingPolicy ile otomatik hesaplanir; DTO'daki TicketPrice/BaggageFee kullanilmaz---//
    public async Task<DataResult<Guid>> CreateReservationWithTicketsAndPaymentAsync(
        CreateReservationDto dto, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating reservation with tickets and payment: UserId {UserId}, TicketCount {TicketCount}", 
            dto.AppUserId, dto.Tickets.Count);

        // SqlServerRetryingExecutionStrategy, kullanici tarafindan baslatilan transaction'i desteklemez; bu yuzden strategy kullanilmiyor.
        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {

            //---PNR olustur---//
            var pnr = string.IsNullOrWhiteSpace(dto.PNR)
                ? Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
                : dto.PNR;

            //---1) Her bilet icin fiyat hesapla, ucus/yolcu/koltuk kontrolleri---//
            // Cache entities to avoid duplicate queries
            var flightCache = new Dictionary<Guid, Flight>();
            var passengerCache = new Dictionary<Guid, Passenger>();
            var calculatedPrices = new List<(decimal ticketPrice, decimal baggageFee)>();
            
            foreach (var ticketDto in dto.Tickets)
            {
                // Ucusu cache'den al veya yukle
                if (!flightCache.TryGetValue(ticketDto.FlightId, out var flight))
                {
                    flight = await _unitOfWork.Flights.GetByIdAsync(ticketDto.FlightId, cancellationToken);
                    if (flight == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return new ErrorDataResult<Guid>(Guid.Empty, $"Ucus bulunamadi: {ticketDto.FlightId}");
                    }
                    flightCache[ticketDto.FlightId] = flight;
                }

                // Yolcuyu cache'den al veya yukle
                if (!passengerCache.TryGetValue(ticketDto.PassengerId, out var passenger))
                {
                    passenger = await _unitOfWork.Passengers.GetByIdAsync(ticketDto.PassengerId, cancellationToken);
                    if (passenger == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return new ErrorDataResult<Guid>(Guid.Empty, $"Yolcu bulunamadi: {ticketDto.PassengerId}");
                    }
                    passengerCache[ticketDto.PassengerId] = passenger;
                }

                if (flight.AvailableSeats <= 0)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Ucus icin yeterli koltuk yok: {flight.FlightNumber}");
                }

                var (ticketPrice, baggageFee) = _pricingPolicy.CalculateTicketPriceAndBaggage(flight, ticketDto.SeatClass, ticketDto.BaggageOption);
                calculatedPrices.Add((ticketPrice, baggageFee));
            }

            //---Toplam: bilet varsa hesaplanan, yoksa DTO'daki TotalPrice---//
            var totalPrice = dto.Tickets.Count > 0
                ? calculatedPrices.Sum(x => x.ticketPrice + x.baggageFee)
                : dto.TotalPrice;

            //---Rezervasyon olustur---//
            var reservation = new Reservation(pnr, dto.AppUserId, totalPrice, dto.Currency, dto.Type);
            
            //---Hotel/Car/Tour ID'lerini set et---//
            if (dto.HotelId.HasValue)
                reservation.SetHotel(dto.HotelId.Value);
            if (dto.CarId.HasValue)
            {
                reservation.SetCar(dto.CarId.Value);
                if (dto.CarPickUpDate.HasValue || dto.CarDropOffDate.HasValue || !string.IsNullOrWhiteSpace(dto.CarPickUpLocation) || !string.IsNullOrWhiteSpace(dto.CarDropOffLocation))
                    reservation.SetCarRentalDetails(dto.CarPickUpDate, dto.CarDropOffDate, dto.CarPickUpLocation, dto.CarDropOffLocation);
            }
            if (dto.TourId.HasValue)
                reservation.SetTour(dto.TourId.Value);
            if (dto.Type == ReservationType.Flight && dto.Tickets.Count == 0 && !string.IsNullOrWhiteSpace(dto.FlightRouteSummary))
                reservation.SetFlightRouteSummary(dto.FlightRouteSummary);
            
            await _validator.ValidateAndThrowAsync(reservation);
            await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //---Rezervasyon ID'sinin set edildigini dogrula---//
            if (reservation.Id == Guid.Empty)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return new ErrorDataResult<Guid>(Guid.Empty, "Rezervasyon ID'si olusturulamadi.");
            }

            //---Rezervasyonun veritabaninda mevcut oldugunu dogrula (foreign key constraint icin)---//
            //---Rezervasyonu veritabanindan yeniden yukle ki EF Core tracking dogru olsun---//
            var savedReservation = await _unitOfWork.Reservations.GetByIdAsync(reservation.Id, cancellationToken);
            if (savedReservation == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return new ErrorDataResult<Guid>(Guid.Empty, "Rezervasyon veritabaninda bulunamadi. Foreign key constraint hatasi onleniyor.");
            }

            //---2) Biletleri olustur (hesaplanan fiyatlarla) ve ucustan koltuklari rezerve et---//
            var tickets = new List<Ticket>();
            for (var i = 0; i < dto.Tickets.Count; i++)
            {
                var ticketDto = dto.Tickets[i];
                var (ticketPrice, baggageFee) = calculatedPrices[i];

                // Cache'den al (zaten yuklendi)
                var flight = flightCache[ticketDto.FlightId];
                var passenger = passengerCache[ticketDto.PassengerId];

                var ticket = new Ticket(
                    ticketDto.FlightId,
                    savedReservation.Id,
                    ticketDto.PassengerId,
                    ticketDto.Email,
                    ticketDto.ContactPhoneNumber,
                    ticketDto.SeatClass,
                    ticketDto.BaggageOption,
                    ticketPrice,
                    baggageFee
                );

                if (!string.IsNullOrWhiteSpace(ticketDto.SeatNumber))
                    ticket.AssignSeat(ticketDto.SeatNumber);

                savedReservation.AddTicket(ticket);
                flight.AddTicket(ticket);
                passenger.AddTicket(ticket);

                tickets.Add(ticket);
                flight.ReserveSeats(1);
                await _unitOfWork.Flights.UpdateAsync(flight, cancellationToken);
            }

            await _unitOfWork.Tickets.AddRangeAsync(tickets, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //---Katilimcilar: Tur/Otel/Arac veya harici ucus (bilet yok) icin Participants'i Passenger olarak ekle---//
            var addParticipants = dto.Participants != null && dto.Participants.Count > 0 &&
                (dto.Type == ReservationType.Tour || dto.Type == ReservationType.Hotel || dto.Type == ReservationType.Car ||
                 (dto.Type == ReservationType.Flight && dto.Tickets.Count == 0));
            if (addParticipants && dto.Participants != null)
            {
                foreach (var pDto in dto.Participants)
                {
                    var passenger = new Passenger(
                        pDto.PassengerFirstName,
                        pDto.PassengerLastName,
                        pDto.NationalNumber ?? string.Empty,
                        pDto.PassportNumber ?? string.Empty,
                        pDto.DateOfBirth,
                        pDto.PassengerType);
                    await _unitOfWork.Passengers.AddAsync(passenger, cancellationToken);
                    savedReservation.AddPassenger(passenger);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            //---Odeme olustur (varsa): hesaplanan totalPrice ve dto.Currency kullanilir---//
            Reservation? existingReservation = null;
            if (dto.Payment != null)
            {
                //---Rezervasyon ID'sinin hala gecerli oldugunu dogrula---//
                if (savedReservation.Id == Guid.Empty)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new ErrorDataResult<Guid>(Guid.Empty, "Rezervasyon ID'si gecersiz. Odeme olusturulamiyor.");
                }

                //---Rezervasyonun hala veritabaninda mevcut oldugunu dogrula---//
                existingReservation = await _unitOfWork.Reservations.GetByIdAsync(savedReservation.Id, cancellationToken);
                if (existingReservation == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Rezervasyon bulunamadi (ID: {savedReservation.Id}). Odeme olusturulamiyor.");
                }

                var money = new Money(totalPrice, dto.Currency);
                var payment = new Payment(
                    existingReservation.Id,
                    money,
                    dto.Payment.PaymentMethod,
                    dto.Payment.TransactionId,
                    dto.Payment.TransactionType
                );

                existingReservation.AddPayment(payment);

                await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                payment.UpdatePaymentStatus(Domain.Enums.PaymentStatus.Paid);
                existingReservation.UpdatePaymentStatus(Domain.Enums.PaymentStatus.Paid);
                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                await _unitOfWork.Reservations.UpdateAsync(existingReservation, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            //---Rezervasyon ID'sini kullan (payment varsa existingReservation, yoksa savedReservation)---//
            var finalReservationId = existingReservation?.Id ?? savedReservation.Id;

            _logger.LogInformation("Reservation created successfully: {ReservationId} - PNR {PNR}, Tickets: {TicketCount}", 
                finalReservationId, savedReservation.PNR, tickets.Count);
            
            return new SuccessDataResult<Guid>(finalReservationId, "Rezervasyon basariyla olusturuldu.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) when (dbEx.InnerException?.Message?.Contains("FK_Payments_Reservations_ReservationId") == true)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(dbEx, "Foreign key constraint violation: Payment cannot be created because Reservation does not exist.");
            return new ErrorDataResult<Guid>(Guid.Empty, "Odeme olusturulamadi: Rezervasyon veritabaninda bulunamadi. Lutfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating reservation with tickets and payment");
            return new ErrorDataResult<Guid>(Guid.Empty, $"Rezervasyon olusturulurken hata olustu: {ex.Message}");
        }
    }

    //---Mevcut rezervasyonu guncelleyen metot---//
    //---Concurrency kontrolu ile---//
    public async Task<Result> UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating reservation: {ReservationId} - PNR {PNR}", reservation.Id, reservation.PNR);
        
        await _validator.ValidateAndThrowAsync(reservation);

        try
        {
            await _unitOfWork.Reservations.UpdateAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reservation updated successfully: {ReservationId} - PNR {PNR}", reservation.Id, reservation.PNR);
            return new SuccessResult("Rezervasyon guncellendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating reservation: {ReservationId} - PNR {PNR}", reservation.Id, reservation.PNR);
            return new ErrorResult("Rezervasyon baska bir kullanici tarafindan guncellenmis. Lutfen sayfayi yenileyip tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation: {ReservationId} - PNR {PNR}", reservation.Id, reservation.PNR);
            return new ErrorResult($"Rezervasyon guncellenirken hata olustu: {ex.Message}");
        }
    }

    //---Rezervasyonu soft delete eden metot---//
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Reservations.SoftDeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Rezervasyon silindi.");
    }

    //---Rezervasyonu iptal eden metot (biletleri, odemeleri ve koltuklari isler)---//
    public async Task<Result> CancelReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling reservation: {ReservationId}", reservationId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            //---Rezervasyonu getir---//
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId, cancellationToken);
            if (reservation == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return new ErrorResult("Rezervasyon bulunamadi.");
            }

            if (reservation.Status == Domain.Enums.ReservationStatus.Cancelled)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return new ErrorResult("Rezervasyon zaten iptal edilmis.");
            }

            //---Biletleri iptal et ve ucustan koltuklari serbest birak---//
            var tickets = await _unitOfWork.Tickets.FindAsync(t => t.ReservationId == reservationId, cancellationToken);
            foreach (var ticket in tickets)
            {
                if (ticket.TicketStatus != Domain.Enums.TicketStatus.Cancelled)
                {
                    ticket.UpdateStatus(Domain.Enums.TicketStatus.Cancelled);
                    await _unitOfWork.Tickets.UpdateAsync(ticket, cancellationToken);

                    //---Ucustan koltuklari serbest birak---//
                    var flight = await _unitOfWork.Flights.GetByIdAsync(ticket.FlightId, cancellationToken);
                    if (flight != null)
                    {
                        flight.ReleaseSeats(1);
                        await _unitOfWork.Flights.UpdateAsync(flight, cancellationToken);
                    }
                }
            }

            //---Odemeleri iptal et (iade islemi simule edilmis)---//
            var hasRefund = false;
            var payments = await _unitOfWork.Payments.FindAsync(p => p.ReservationId == reservationId, cancellationToken);
            foreach (var payment in payments)
            {
                if (payment.PaymentStatus == Domain.Enums.PaymentStatus.Paid)
                {
                    //---Iade islemi icin yeni bir payment olustur (TransactionType.Refund)---//
                    var refundMoney = new Money(payment.TransactionAmount.Amount, payment.TransactionAmount.Currency);
                    var refundPayment = new Payment(
                        reservationId,
                        refundMoney,
                        payment.PaymentMethod,
                        $"REFUND-{payment.TransactionId}",
                        Domain.Enums.TransactionType.Refund
                    );
                    refundPayment.UpdatePaymentStatus(Domain.Enums.PaymentStatus.Refunded);
                    await _unitOfWork.Payments.AddAsync(refundPayment, cancellationToken);
                    hasRefund = true;
                }
                else if (payment.PaymentStatus == Domain.Enums.PaymentStatus.Pending)
                {
                    payment.UpdatePaymentStatus(Domain.Enums.PaymentStatus.Failed, "Rezervasyon iptal edildi.");
                    await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                }
            }

            //---Rezervasyon durumunu iptal olarak guncelle---//
            reservation.Cancel();
            if (hasRefund)
                reservation.UpdatePaymentStatus(Domain.Enums.PaymentStatus.Refunded);
            await _unitOfWork.Reservations.UpdateAsync(reservation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Reservation cancelled successfully: {ReservationId}", reservationId);
            return new SuccessResult("Rezervasyon iptal edildi.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error cancelling reservation: {ReservationId}", reservationId);
            return new ErrorResult($"Rezervasyon iptal edilirken hata olustu: {ex.Message}");
        }
    }
}

