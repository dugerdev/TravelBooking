using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Application.Services;

/// <summary>
/// Odemelere iliskin is kurallarini yoneten servis.
/// Odeme olusturma, guncelleme ve sorgulama islemlerini yonetir.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;

    /// <summary>
    /// PaymentService constructor.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work instance.</param>
    /// <param name="logger">Logger instance.</param>
    public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// ID'ye gore odeme getirir. Reservation navigation property'sini de yukler.
    /// </summary>
    /// <param name="id">Odeme ID'si.</param>
    /// <param name="cancellationToken">Iptal token'i.</param>
    /// <returns>Odeme bilgileri veya hata mesaji.</returns>
    public async Task<DataResult<Payment>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include Reservation navigation property for authorization checks
        var payment = await _unitOfWork.Context.Set<Payment>()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            _logger.LogWarning("Payment not found with id: {PaymentId}", id);
            return new ErrorDataResult<Payment>(null!, "Odeme bulunamadi.");
        }

        return new SuccessDataResult<Payment>(payment);
    }

    //---Rezervasyon ID'sine gore odemeleri getiren metot---//
    public async Task<DataResult<IEnumerable<Payment>>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        // Include Reservation navigation property to avoid N+1 queries
        var payments = await _unitOfWork.Context.Set<Payment>()
            .Where(p => p.ReservationId == reservationId && !p.IsDeleted)
            .Include(p => p.Reservation)
            .ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Payment>>(payments);
    }

    //---Tum odemeleri getiren metot---//
    public async Task<DataResult<IEnumerable<Payment>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetAllAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Payment>>(payments);
    }

    //---Tum odemeleri pagination ile getiren metot---//
    public async Task<DataResult<PagedResult<Payment>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting payments with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Payments.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Payment>>(pagedResult);
    }

    //---Yeni odeme ekleyen metot---//
    public async Task<Result> AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new payment: ReservationId {ReservationId}, Amount {Amount}", payment.ReservationId, payment.TransactionAmount.Amount);
        
        //---Rezervasyonun mevcut oldugunu dogrula---//
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(payment.ReservationId, cancellationToken);
        if (reservation == null)
        {
            _logger.LogWarning("Payment cannot be added: Reservation not found. ReservationId: {ReservationId}", payment.ReservationId);
            return new ErrorResult($"Odeme eklenemedi: Rezervasyon bulunamadi (ID: {payment.ReservationId}).");
        }
        
        try
        {
            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment added successfully: {PaymentId} - TransactionId {TransactionId}", payment.Id, payment.TransactionId);
            return new SuccessResult("Odeme eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) when (dbEx.InnerException?.Message?.Contains("FK_Payments_Reservations_ReservationId") == true)
        {
            _logger.LogError(dbEx, "Foreign key constraint violation: Payment cannot be created because Reservation does not exist. ReservationId: {ReservationId}", payment.ReservationId);
            return new ErrorResult($"Odeme eklenemedi: Rezervasyon bulunamadi (ID: {payment.ReservationId}). Lutfen rezervasyonun mevcut oldugundan emin olun.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment: ReservationId {ReservationId}", payment.ReservationId);
            return new ErrorResult($"Odeme eklenirken hata olustu: {ex.Message}");
        }
    }

    //---Mevcut odemeyi guncelleyen metot---//
    public async Task<Result> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating payment: {PaymentId}", payment.Id);
        
        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment updated successfully: {PaymentId}", payment.Id);
        return new SuccessResult("Odeme guncellendi.");
    }

    //---Odeme islemi yapan metot---//
    public async Task<Result> ProcessPaymentAsync(
        Guid reservationId, 
        decimal amount, 
        Domain.Enums.PaymentMethod paymentMethod, 
        string transactionId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            transactionId = "TXN-" + Guid.NewGuid().ToString("N");

        _logger.LogInformation("Processing payment for reservation: {ReservationId}, Amount: {Amount}, Method: {Method}", 
            reservationId, amount, paymentMethod);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            //---Rezervasyonu kontrol et---//
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId, cancellationToken);
            if (reservation == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return new ErrorResult("Rezervasyon bulunamadi.");
            }

            //---Odeme olustur---//
            var money = new Domain.Common.Money(amount, reservation.Currency);
            var payment = new Payment(
                reservationId,
                money,
                paymentMethod,
                transactionId,
                Domain.Enums.TransactionType.Payment
            );

            //---Navigation property'yi set et---//
            reservation.AddPayment(payment);

            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);

            //---Odeme durumunu guncelle (simule edilmis - gercek odeme gateway entegrasyonu gerekir)---//
            //---Burada odeme gateway'den gelen response'a gore durum guncellenmeli---//
            payment.UpdatePaymentStatus(Domain.Enums.PaymentStatus.Paid);
            reservation.UpdatePaymentStatus(Domain.Enums.PaymentStatus.Paid);

            await _unitOfWork.Reservations.UpdateAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Payment processed successfully: {PaymentId} - TransactionId {TransactionId}", payment.Id, transactionId);
            return new SuccessResult("Odeme islemi basariyla tamamlandi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) when (dbEx.InnerException?.Message?.Contains("FK_Payments_Reservations_ReservationId") == true)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(dbEx, "Foreign key constraint violation: Payment cannot be created because Reservation does not exist. ReservationId: {ReservationId}", reservationId);
            return new ErrorResult($"Odeme olusturulamadi: Rezervasyon bulunamadi (ID: {reservationId}). Lutfen rezervasyonun mevcut oldugundan emin olun.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error processing payment for reservation: {ReservationId}", reservationId);
            return new ErrorResult($"Odeme islemi sirasinda hata olustu: {ex.Message}");
        }
    }
}
