using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Identity;
using TravelBooking.Infrastructure.Data;
using TravelBooking.Infrastructure.Repositories;
using TravelBooking.Infrastructure.External;
using Microsoft.AspNetCore.Identity;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace TravelBooking.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TravelBookingDbContext>(options =>
        {
            var commandTimeout = configuration.GetValue<int>("Database:CommandTimeoutSeconds", 60);
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions => sqlServerOptions.CommandTimeout(commandTimeout));
            // Migration uygulanirken snapshot ile model arasindaki kucuk farklarda hata vermesin
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // Identity (library-friendly): use AddIdentityCore like TechCommerce
        services.AddIdentityCore<AppUser>(options =>
            {
                // Password policy (basic, can be tightened)
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout policy
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

                // User policy
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TravelBookingDbContext>();

        // Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Email Service
        services.AddScoped<IEmailService, EmailService>();

        //---External API Client (Aviationstack - eski, artik kullanilmiyor)---//
        services.AddHttpClient("FlightApi", client =>
        {
            var baseUrl = configuration["FlightApi:BaseUrl"] ?? "https://api.aviationstack.com/v1";
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FlightApi:TimeoutSeconds", 30));
        }).AddPolicyHandler(GetRetryPolicy()).AddPolicyHandler(GetCircuitBreakerPolicy());

        //---AeroDataBox (RapidAPI) - FIDS ile kalkis/varis IATA + tarih aramasi---//
        var aeroKey = configuration["AeroDataBox:RapidAPIKey"];
        var aeroBase = configuration["AeroDataBox:BaseUrl"] ?? "https://aerodatabox.p.rapidapi.com";
        services.AddHttpClient("AeroDataBox", client =>
        {
            client.BaseAddress = new Uri(aeroBase.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("AeroDataBox:TimeoutSeconds", 60));
            if (!string.IsNullOrWhiteSpace(aeroKey))
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", aeroKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "aerodatabox.p.rapidapi.com");
            }
        }).AddPolicyHandler(GetRetryPolicy()).AddPolicyHandler(GetCircuitBreakerPolicy());
        
        services.AddScoped<IExternalFlightApiClient, ExternalFlightApiClient>();

        return services;
    }

    //---Gecici HTTP hatalari icin retry policy---//
    //---Not: TaskCanceledException'i retry ETME (timeout'lar retry edilmemeli)---//
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            // TaskCanceledException retry edilmez - timeout'lar icin retry yapilmaz
            .WaitAndRetryAsync(
                retryCount: 2, // 3'ten 2'ye dusuruldu (toplam timeout suresini azaltmak icin)
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Min(2, Math.Pow(2, retryAttempt))), // Max 2 saniye bekleme
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    //---Retry denemelerini logla (gerekirse)---//
                });
    }

    //---Zincirleme hatalari onlemek icin circuit breaker policy---//
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
