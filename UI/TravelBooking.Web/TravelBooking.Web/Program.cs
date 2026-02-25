using System.Globalization;
using System.Linq;
using TravelBooking.Web.Configuration;
using TravelBooking.Web.Filters;
using TravelBooking.Web.Helpers;
using TravelBooking.Web.Middleware;
using TravelBooking.Web.Services.Auth;
using TravelBooking.Web.Services.Account;
using TravelBooking.Web.Services.Admin;
using TravelBooking.Web.Services.Settings;
using TravelBooking.Web.Services.Flights;
using TravelBooking.Web.Services.Airports;
using TravelBooking.Web.Services.Reservations;
using TravelBooking.Web.Services.Passengers;
using TravelBooking.Web.Services.Hotels;
using TravelBooking.Web.Services.Cars;
using TravelBooking.Web.Services.Tours;
using TravelBooking.Web.Services.News;
using TravelBooking.Web.Services.TravelBookingApi;
using TravelBooking.Web.Services.Currency;
using TravelBooking.Web.Services.ContactMessages;
using TravelBooking.Web.Services.Email;
using TravelBooking.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

//Örnek: Oturum destegi - ucus listesi gibi buyuk veriler icin bellek cache kullanilir
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<AdminSidebarFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LocalizationActionFilter>();
    options.Filters.Add<AdminSidebarFilter>();
})
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddHttpContextAccessor();

//Örnek: [Authorize] ve Admin alani icin cookie tabanli kimlik dogrulama; TokenAuthenticationMiddleware token cookie'den User set eder
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.Configure<TravelBookingApiOptions>(builder.Configuration.GetSection(TravelBookingApiOptions.SectionName));
builder.Services.Configure<AuthCookieOptions>(builder.Configuration.GetSection(AuthCookieOptions.SectionName));
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));

builder.Services.AddScoped<ICookieHelper, CookieHelper>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IViewCurrencyHelper, ViewCurrencyHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IAirportService, AirportService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPassengerService, PassengerService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IReservationEmailService, ReservationEmailService>();
builder.Services.AddTransient<TravelBookingApiAuthHandler>();

//Örnek: API cagrilarinda token header eklenir (TravelBookingApiAuthHandler), hata durumunda retry ve circuit breaker devreye girer
builder.Services.AddHttpClient<ITravelBookingApiClient, TravelBookingApiClient>()
    .AddHttpMessageHandler<TravelBookingApiAuthHandler>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<TestimonialService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<TravelBookingApiOptions>>();
    client.BaseAddress = new Uri(config.Value.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

//Örnek: Gecici HTTP hatalarinda (5xx, timeout) en fazla 2 kez tekrar dener; 2^n saniye bekleyerek (exponential backoff)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

//Örnek: 5 ard arda hata sonrasi devre acar, 30 saniye boyunca API cagrilari engellenir
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}

var app = builder.Build();

// Stripe API key (backend only)
var stripeOptions = app.Services.GetRequiredService<IOptions<StripeOptions>>().Value;
if (!string.IsNullOrEmpty(stripeOptions.SecretKey))
    global::Stripe.StripeConfiguration.ApiKey = stripeOptions.SecretKey;

// Localization konfigurasyonu - cookie first so user language choice is applied
var supportedCultures = new[] { "tr", "en" };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
};
localizationOptions.RequestCultureProviders.Insert(0, new Microsoft.AspNetCore.Localization.CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

//Örnek: Middleware sirasi onemli - Exception en basta, Session Authentication'dan once, TokenAuth Authorization'dan once
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); //Örnek: Ucus listesi gibi buyuk veriler icin session
app.UseAuthentication();
app.UseMiddleware<LocalizationMiddleware>();
app.UseMiddleware<TokenAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();