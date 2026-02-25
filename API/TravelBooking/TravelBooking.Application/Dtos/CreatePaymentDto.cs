using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class CreatePaymentDto
{
    /// <summary>Yeni rezervasyon olustururken backend doldurur; frontend null veya atlayabilir.</summary>
    public Guid? ReservationId { get; set; }
    public decimal TransactionAmount { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;
    public string TransactionId { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; } = TransactionType.Payment;
}
