using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Services;
using TravelBooking.Application.Services.External;
using TravelBooking.Application.Services.Pricing;
using TravelBooking.Application.Validators;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Services;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IFlightService, FlightManager>();
        services.AddScoped<IReservationService, ReservationManager>();
        services.AddScoped<IAirportService, AirportManager>();
        services.AddScoped<IPassengerService, PassengerManager>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IHotelService, HotelManager>();
        services.AddScoped<ICarService, CarManager>();
        services.AddScoped<ITourService, TourManager>();
        services.AddScoped<INewsService, NewsManager>();
        services.AddScoped<IContactMessageService, ContactMessageManager>();
        services.AddScoped<ITestimonialService, TestimonialManager>();

        // Domain Event Handlers
        services.AddScoped<TravelBooking.Domain.Events.IDomainEventHandler<TravelBooking.Domain.Events.ReservationConfirmedEvent>, TravelBooking.Application.Handlers.ReservationConfirmedEventHandler>();
        services.AddScoped<TravelBooking.Domain.Events.IDomainEventHandler<TravelBooking.Domain.Events.PaymentFailedEvent>, TravelBooking.Application.Handlers.PaymentFailedEventHandler>();
        services.AddScoped<TravelBooking.Domain.Events.IDomainEventHandler<TravelBooking.Domain.Events.TicketCancelledEvent>, TravelBooking.Application.Handlers.TicketCancelledEventHandler>();
        services.AddScoped<TravelBooking.Domain.Events.IDomainEventHandler<TravelBooking.Domain.Events.SeatsReservedEvent>, TravelBooking.Application.Handlers.SeatsReservedEventHandler>();
        
        // User Management Services
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        
        // External Services
        services.AddScoped<IFlightDataSyncService, FlightDataSyncService>();

        // Fiyatlandirma
        services.AddScoped<IPricingPolicy, DefaultPricingPolicy>();

        // Entity Validators (Domain katmani icin)
        services.AddScoped<IValidator<Flight>, FlightValidator>();
        services.AddScoped<IValidator<Reservation>, ReservationValidator>();
        services.AddScoped<IValidator<Airport>, AirportValidator>();
        services.AddScoped<IValidator<Passenger>, PassengerValidator>();
        services.AddScoped<IValidator<Car>, CarValidator>();
        services.AddScoped<IValidator<Hotel>, HotelValidator>();
        services.AddScoped<IValidator<Tour>, TourValidator>();
        services.AddScoped<IValidator<NewsArticle>, NewsArticleValidator>();

        // DTO Validators (API katmani icin gelen request'leri validate eder)
        services.AddScoped<IValidator<CreateFlightDto>, CreateFlightDtoValidator>();
        services.AddScoped<IValidator<CreateReservationDto>, CreateReservationDtoValidator>();
        services.AddScoped<IValidator<CreatePassengerDto>, CreatePassengerDtoValidator>();
        services.AddScoped<IValidator<CreateAirportDto>, CreateAirportDtoValidator>();
        services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
        services.AddScoped<IValidator<UpdateProfileDto>, UpdateProfileDtoValidator>();
        services.AddScoped<IValidator<CreateTicketDto>, CreateTicketDtoValidator>();
        services.AddScoped<IValidator<CreatePaymentDto>, CreatePaymentDtoValidator>();
        services.AddScoped<IValidator<CreateCarDto>, CreateCarDtoValidator>();
        services.AddScoped<IValidator<CreateHotelDto>, CreateHotelDtoValidator>();
        services.AddScoped<IValidator<CreateTourDto>, CreateTourDtoValidator>();
        services.AddScoped<IValidator<CreateNewsDto>, CreateNewsDtoValidator>();

        return services;
    }
}
