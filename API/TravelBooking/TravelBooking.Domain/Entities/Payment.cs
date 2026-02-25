using TravelBooking.Domain.Common;
using System;
using TravelBooking.Domain.Enums;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a payment entity in the domain.
/// Tracks payment transactions associated with reservations.
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// Gets the ID of the reservation this payment is associated with.
    /// </summary>
    public Guid ReservationId { get; private set; }

    /// <summary>
    /// Gets the navigation property to the reservation.
    /// </summary>
    public Reservation Reservation { get; private set; } = null!;

    /// <summary>
    /// Gets the transaction amount for this payment.
    /// </summary>
    public Money TransactionAmount { get; private set; } = null!;

    /// <summary>
    /// Gets the current payment status.
    /// </summary>
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;

    /// <summary>
    /// Gets the payment method used for this transaction.
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; } = PaymentMethod.Card;

    /// <summary>
    /// Gets the transaction ID from the bank or payment service provider.
    /// </summary>
    public string TransactionId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the type of transaction (Payment, Refund, etc.).
    /// </summary>
    public TransactionType TransactionType { get; private set; } = TransactionType.Payment;

    /// <summary>
    /// Gets the error message, if any, associated with a failed payment.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the date and time when the transaction occurred.
    /// </summary>
    public DateTime TransactionDate { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Protected parameterless constructor for Entity Framework.
    /// </summary>
    protected Payment() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Payment"/> class.
    /// </summary>
    /// <param name="reservationId">The ID of the associated reservation.</param>
    /// <param name="transactionAmount">The amount of the transaction.</param>
    /// <param name="paymentMethod">The payment method used.</param>
    /// <param name="transactionId">The transaction ID from the payment provider.</param>
    /// <param name="transactionType">The type of transaction.</param>
    public Payment(
        Guid reservationId,
        Money transactionAmount,
        PaymentMethod paymentMethod,
        string transactionId,
        TransactionType transactionType)
    {
        ReservationId = reservationId;
        TransactionAmount = transactionAmount;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        TransactionType = transactionType;
        TransactionDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the payment status and optionally sets an error message.
    /// </summary>
    /// <param name="status">The new payment status.</param>
    /// <param name="errorMessage">Optional error message for failed payments.</param>
    public void UpdatePaymentStatus(PaymentStatus status, string? errorMessage = null)
    {
        PaymentStatus = status;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Updates the transaction ID.
    /// </summary>
    /// <param name="transactionId">The new transaction ID.</param>
    public void UpdateTransactionId(string transactionId)
    {
        TransactionId = transactionId;
    }

    /// <summary>
    /// Sets the navigation property to the reservation.
    /// </summary>
    /// <param name="reservation">The reservation to associate with this payment.</param>
    internal void SetReservation(Reservation reservation)
    {
        Reservation = reservation;
    }
}
