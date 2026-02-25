using System.Linq;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Services;

namespace TravelBooking.Application.Services.Pricing;

/// <summary>Ucus BasePrice, SeatClass ve BaggageOption'a gore otomatik fiyat hesaplar.</summary>
public sealed class DefaultPricingPolicy : IPricingPolicy
{
    public Money CalculateTotalPrice(Reservation reservation)
    {
        var sum = reservation.Tickets?.Sum(t => t.TicketPrice + t.BaggageFee) ?? 0m;
        return new Money(sum, reservation.Currency);
    }

    public (decimal ticketPrice, decimal baggageFee) CalculateTicketPriceAndBaggage(
        Flight flight, SeatClass seatClass, BaggageOption baggageOption)
    {
        var baseAmount = flight.BasePrice?.Amount ?? 0m;
        if (baseAmount < 0) baseAmount = 0;

        var multiplier = seatClass switch
        {
            SeatClass.Economy => 1.0m,
            SeatClass.PremiumEconomy => 1.3m,
            SeatClass.Business => 2.0m,
            SeatClass.First => 3.0m,
            _ => 1.0m
        };

        var baggageFee = baggageOption switch
        {
            BaggageOption.Light => 0m,
            BaggageOption.Standard => 50m,
            BaggageOption.Plus => 100m,
            BaggageOption.Business => 0m,
            _ => 0m
        };

        var ticketPrice = Math.Round(baseAmount * multiplier, 2);
        return (ticketPrice, baggageFee);
    }
}
