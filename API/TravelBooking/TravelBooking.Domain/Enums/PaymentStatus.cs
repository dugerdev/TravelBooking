

namespace TravelBooking.Domain.Enums;

public enum PaymentStatus

{
    Pending = 1,                         // Odeme bekleniyor
    Paid = 2,                           // Odeme basarili
    Failed = 3,                        // Odeme basarisiz
    Refunded = 4                      // Odeme iade edildi

}
