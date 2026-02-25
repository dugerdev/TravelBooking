using TravelBooking.Application.Dtos.External;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Common;

namespace TravelBooking.Application.Mappings;

public static class FlightMappingExtensions
{
    public static Flight ToDomainEntity(
        this ExternalFlightDto dto, 
        Guid departureAirportId, 
        Guid arrivalAirportId)
    {
        var dep = dto.ScheduledDeparture;
        var arr = dto.ScheduledArrival;
        if (arr <= dep)
            arr = dep.AddHours(1);

        var basePrice = new Money(dto.BasePriceAmount, ParseCurrency(dto.Currency));
        var flightType = ParseFlightType(dto.FlightType);
        var flightRegion = ParseFlightRegion(dto.FlightRegion);

        var flight = new Flight(
            dto.FlightNumber,
            dto.AirlineName,
            departureAirportId,
            arrivalAirportId,
            dep,
            arr,
            basePrice,
            dto.TotalSeats,
            flightType,
            flightRegion
        );

        //---External API'den gelen AvailableSeats degerini guncelle---//
        if (dto.AvailableSeats > 0 && dto.AvailableSeats <= dto.TotalSeats)
        {
            flight.UpdateAvailableSeats(dto.AvailableSeats);
        }

        return flight;
    }

    private static Currency ParseCurrency(string currency)
    {
        return currency.ToUpperInvariant() switch
        {
            "TRY" => Currency.TRY,
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            _ => Currency.TRY
        };
    }

    private static FlightType ParseFlightType(string flightType)
    {
        return flightType.ToUpperInvariant() switch
        {
            "DIRECT" => FlightType.Direct,
            "CONNECTING" => FlightType.Connecting,
            "TRANSIT" => FlightType.Connecting, // Transit is mapped to Connecting
            "CHARTER" => FlightType.Charter,
            _ => FlightType.Direct
        };
    }

    private static FlightRegion ParseFlightRegion(string flightRegion)
    {
        return flightRegion.ToUpperInvariant() switch
        {
            "DOMESTIC" => FlightRegion.Domestic,
            "INTERNATIONAL" => FlightRegion.International,
            _ => FlightRegion.Domestic
        };
    }
}
