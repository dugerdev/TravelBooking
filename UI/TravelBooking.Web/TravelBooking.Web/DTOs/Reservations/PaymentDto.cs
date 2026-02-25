using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.DTOs.Reservations;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public decimal TransactionAmount { get; set; }
    public Currency Currency { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
