using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.DTOs.Reservations;

public class CreatePaymentDto
{
    public Guid? ReservationId { get; set; }
    public decimal TransactionAmount { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;
    public string TransactionId { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; } = TransactionType.Payment;
}
