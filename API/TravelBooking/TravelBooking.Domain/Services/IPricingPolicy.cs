using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;

namespace TravelBooking.Domain.Services;

//---Fiyatlandirma politikasi icin domain service interface'i---//
public interface IPricingPolicy
{
    /// <summary>Mevcut rezervasyondaki biletlerin toplamini dondurur.</summary>
    Money CalculateTotalPrice(Reservation reservation);

    /// <summary>Ucus BasePrice + koltuk sinifi + bagaj secenegine gore bilet ve bagaj ucreti hesaplar.</summary>
    (decimal ticketPrice, decimal baggageFee) CalculateTicketPriceAndBaggage(Flight flight, SeatClass seatClass, BaggageOption baggageOption);
}
