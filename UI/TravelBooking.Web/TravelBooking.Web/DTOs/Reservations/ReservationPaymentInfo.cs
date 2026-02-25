using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.DTOs.Reservations;

/// <summary>
/// Payment summary for reservation display (Manage Booking, etc.)
/// </summary>
public class ReservationPaymentInfo
{
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
}
