using AutoMapper;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Application.Dtos.External;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Flight mappings
        CreateMap<Flight, FlightDto>()
            .ForMember(dest => dest.BasePriceAmount, opt => opt.MapFrom(src => src.BasePrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.BasePrice.Currency.ToString()))
            .ForMember(dest => dest.DepartureAirport, opt => opt.MapFrom(src => src.DepartureAirport))
            .ForMember(dest => dest.ArrivalAirport, opt => opt.MapFrom(src => src.ArrivalAirport))
            .ForMember(dest => dest.DepartureAirportName, opt => opt.MapFrom(src => src.DepartureAirport != null ? src.DepartureAirport.Name : null))
            .ForMember(dest => dest.ArrivalAirportName, opt => opt.MapFrom(src => src.ArrivalAirport != null ? src.ArrivalAirport.Name : null))
            .ForMember(dest => dest.DepartureAirportIATA, opt => opt.MapFrom(src => src.DepartureAirport != null ? src.DepartureAirport.IATA_Code : null))
            .ForMember(dest => dest.ArrivalAirportIATA, opt => opt.MapFrom(src => src.ArrivalAirport != null ? src.ArrivalAirport.IATA_Code : null));

        // Reservation mappings
        CreateMap<Reservation, ReservationDto>()
            .ForMember(dest => dest.AppUserId, opt => opt.MapFrom(src => src.AppUserId == null ? string.Empty : src.AppUserId))
            .ForMember(dest => dest.Tickets, opt => opt.MapFrom(src => src.Tickets != null ? src.Tickets : new List<Ticket>()))
            .ForMember(dest => dest.Passengers, opt => opt.MapFrom(src =>
                (src.Tickets != null ? src.Tickets.Where(t => t.Passenger != null).Select(t => t.Passenger) : Enumerable.Empty<Passenger>())
                .Concat(src.Passengers ?? Enumerable.Empty<Passenger>()).Distinct()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.AppUser != null ? (src.AppUser.UserName != null ? src.AppUser.UserName : (src.AppUser.Email != null ? src.AppUser.Email : "")) : ""))
            .ForMember(dest => dest.ReservationType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.HotelId))
            .ForMember(dest => dest.CarId, opt => opt.MapFrom(src => src.CarId))
            .ForMember(dest => dest.TourId, opt => opt.MapFrom(src => src.TourId))
            .ForMember(dest => dest.ReservationSummary, opt => opt.MapFrom(src =>
                src.Type == ReservationType.Flight ? 
                    (src.Tickets != null && src.Tickets.Any() && src.Tickets.First().Flight != null &&
                     src.Tickets.First().Flight.DepartureAirport != null && src.Tickets.First().Flight.ArrivalAirport != null ?
                        $"{src.Tickets.First().Flight.DepartureAirport.IATA_Code} → {src.Tickets.First().Flight.ArrivalAirport.IATA_Code}" :
                        !string.IsNullOrWhiteSpace(src.FlightRouteSummary) ? src.FlightRouteSummary! : "Ucus") :
                src.Type == ReservationType.Hotel && src.Hotel != null ? src.Hotel.Name :
                src.Type == ReservationType.Car && src.Car != null ? $"{src.Car.Brand} {src.Car.Model}" :
                src.Type == ReservationType.Tour && src.Tour != null ? src.Tour.Name :
                "-"
            ))
            .ForMember(dest => dest.Flight, opt => opt.MapFrom(src =>
                src.Type == ReservationType.Flight && src.Tickets != null && src.Tickets.Any() ? src.Tickets.First().Flight : null))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments != null ? src.Payments : new List<Payment>()));

        // Ticket mappings
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.SeatNumber ?? string.Empty))
            .ForMember(dest => dest.Passenger, opt => opt.MapFrom(src => src.Passenger));

        // Passenger mappings
        CreateMap<Passenger, PassengerDto>();

        // Airport mappings (City bossa IATA'dan fallback)
        CreateMap<Airport, AirportDto>()
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => AirportCityFallback.ResolveCity(src.City, src.IATA_Code)));

        // Payment mappings
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.TransactionAmount, opt => opt.MapFrom(src => src.TransactionAmount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.TransactionAmount.Currency));

        // Flight -> ExternalFlightDto (search-external DB fallback)
        CreateMap<Flight, ExternalFlightDto>()
            .ForMember(dest => dest.ExternalFlightId, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.DepartureAirportIATA, opt => opt.MapFrom(src => src.DepartureAirport != null ? src.DepartureAirport.IATA_Code : ""))
            .ForMember(dest => dest.DepartureAirportName, opt => opt.MapFrom(src => src.DepartureAirport != null ? src.DepartureAirport.Name : null))
            .ForMember(dest => dest.DepartureCity, opt => opt.MapFrom(src => src.DepartureAirport != null ? src.DepartureAirport.City : null))
            .ForMember(dest => dest.DepartureCountry, opt => opt.MapFrom(src => src.DepartureAirport != null ? src.DepartureAirport.Country : null))
            .ForMember(dest => dest.ArrivalAirportIATA, opt => opt.MapFrom(src => src.ArrivalAirport != null ? src.ArrivalAirport.IATA_Code : ""))
            .ForMember(dest => dest.ArrivalAirportName, opt => opt.MapFrom(src => src.ArrivalAirport != null ? src.ArrivalAirport.Name : null))
            .ForMember(dest => dest.ArrivalCity, opt => opt.MapFrom(src => src.ArrivalAirport != null ? src.ArrivalAirport.City : null))
            .ForMember(dest => dest.ArrivalCountry, opt => opt.MapFrom(src => src.ArrivalAirport != null ? src.ArrivalAirport.Country : null))
            .ForMember(dest => dest.BasePriceAmount, opt => opt.MapFrom(src => src.BasePrice != null ? src.BasePrice.Amount : 0))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.BasePrice != null ? src.BasePrice.Currency.ToString() : "TRY"))
            .ForMember(dest => dest.FlightType, opt => opt.MapFrom(src => src.FlightType.ToString()))
            .ForMember(dest => dest.FlightRegion, opt => opt.MapFrom(src => src.FlightRegion.ToString()));

        // Hotel mappings
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.PricePerNight, opt => opt.MapFrom(src => src.PricePerNight.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.PricePerNight.Currency.ToString()))
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms));
        CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

        // Car mappings
        CreateMap<Car, CarDto>()
            .ForMember(dest => dest.PricePerDay, opt => opt.MapFrom(src => src.PricePerDay.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.PricePerDay.Currency.ToString()));

        // Tour mappings
        CreateMap<Tour, TourDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency.ToString()))
            .ForMember(dest => dest.Highlights, opt => opt.MapFrom(src => src.Highlights))
            .ForMember(dest => dest.Included, opt => opt.MapFrom(src => src.Included));

        // News mappings
        CreateMap<NewsArticle, NewsDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

        // ContactMessage mappings
        CreateMap<ContactMessage, ContactMessageDto>();

        // Testimonial mappings
        CreateMap<Testimonial, TestimonialDto>();
    }
}
