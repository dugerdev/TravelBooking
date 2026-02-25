using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels.Tours;
using TravelBooking.Web.ViewModels.Payments;
using TravelBooking.Web.Services.Tours;
using TravelBooking.Web.Services.Reservations;
using TravelBooking.Web.Services.Auth;
using TravelBooking.Web.Services.Currency;
using TravelBooking.Web.Helpers;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.DTOs.Enums;
using ReservationType = TravelBooking.Web.DTOs.Enums.ReservationType;

namespace TravelBooking.Web.Controllers;

public class TourController : Controller
{
    private readonly ITourService _tourService;
    private readonly IReservationService _reservationService;
    private readonly IAuthService _authService;
    private readonly ICookieHelper _cookieHelper;
    private readonly ICurrencyService _currencyService;

    public TourController(
        ITourService tourService,
        IReservationService reservationService,
        IAuthService authService,
        ICookieHelper cookieHelper,
        ICurrencyService currencyService)
    {
        _tourService = tourService;
        _reservationService = reservationService;
        _authService = authService;
        _cookieHelper = cookieHelper;
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Detail(Guid id, CancellationToken ct = default)
    {
        var (success, message, tourDto) = await _tourService.GetByIdAsync(id, ct);
        
        if (!success || tourDto == null)
        {
            // Fallback to first tour
            var (allSuccess, _, tours) = await _tourService.GetAllAsync(ct);
            if (allSuccess && tours.Any())
            {
                tourDto = tours.First();
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index", "Home");
            }
        }

        var tour = new TourViewModel
        {
            Id = (int)(tourDto.Id.GetHashCode() & 0x7FFFFFFF),
            RawId = tourDto.Id,
            Name = tourDto.Name,
            Destination = tourDto.Destination,
            Duration = tourDto.Duration,
            Price = tourDto.Price,
            Currency = tourDto.Currency,
            ImageUrl = tourDto.ImageUrl,
            Description = tourDto.Description,
            Highlights = tourDto.Highlights,
            Included = tourDto.Included,
            Rating = tourDto.Rating,
            ReviewCount = tourDto.ReviewCount,
            Difficulty = tourDto.Difficulty,
            MaxGroupSize = tourDto.MaxGroupSize
        };

        var model = new TourDetailViewModel
        {
            Tour = tour,
            Itinerary = new List<string>(),
            Reviews = new List<TourReviewViewModel>(),
            AvailableDates = new List<DateTime>()
        };

        return View(model);
    }

    public async Task<IActionResult> Packages(string? destination, int? minDuration, int? maxDuration, int page = 1, CancellationToken ct = default)
    {
        const int pageSize = 10;
        List<TourPackageViewModel> packages;
        int totalCount;
        int totalPages;

        var hasSearch = !string.IsNullOrWhiteSpace(destination) || minDuration.HasValue || maxDuration.HasValue;

        if (hasSearch)
        {
            var (success, message, tours) = await _tourService.SearchAsync(destination, minDuration, maxDuration, ct);
            packages = tours.Select(t => new TourPackageViewModel
            {
                Id = (int)(t.Id.GetHashCode() & 0x7FFFFFFF),
                RawId = t.Id,
                PackageName = t.Name,
                Description = t.Description,
                Price = t.Price,
                Currency = t.Currency,
                Duration = t.Duration,
                Destinations = new List<string> { t.Destination },
                Included = t.Included,
                ImageUrl = t.ImageUrl
            }).ToList();
            totalCount = packages.Count;
            totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            if (!success)
                TempData["ErrorMessage"] = message;
        }
        else
        {
            var (success, message, paged) = await _tourService.GetAllPagedAsync(page, pageSize, ct);
            if (!success || paged?.Items == null)
            {
                packages = new List<TourPackageViewModel>();
                totalCount = 0;
                totalPages = 0;
                if (!string.IsNullOrEmpty(message))
                    TempData["ErrorMessage"] = message;
            }
            else
            {
                packages = paged.Items.Select(t => new TourPackageViewModel
                {
                    Id = (int)(t.Id.GetHashCode() & 0x7FFFFFFF),
                    RawId = t.Id,
                    PackageName = t.Name,
                    Description = t.Description,
                    Price = t.Price,
                    Currency = t.Currency,
                    Duration = t.Duration,
                    Destinations = new List<string> { t.Destination },
                    Included = t.Included,
                    ImageUrl = t.ImageUrl
                }).ToList();
                totalCount = paged.TotalCount;
                totalPages = paged.TotalPages;
            }
        }

        var model = new TourPackagesViewModel
        {
            Packages = packages,
            SearchDestination = destination,
            MinDuration = minDuration,
            MaxDuration = maxDuration,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return View(model);
    }

    public async Task<IActionResult> Booking(Guid tourId, DateTime? tourDate, int participants = 1, CancellationToken ct = default)
    {
        var (success, message, tourDto) = await _tourService.GetByIdAsync(tourId, ct);
        
        if (!success || tourDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Packages));
        }

        var participantCount = Math.Clamp(participants, 1, 20);
        const int maxPassengers = 20;
        var passengers = new List<TourPassengerViewModel>(maxPassengers);
        for (var i = 0; i < maxPassengers; i++)
            passengers.Add(new TourPassengerViewModel());

        // Get selected currency from cookie
        var selectedCurrency = _cookieHelper.GetCurrency();
        var priceInSelectedCurrency = _currencyService.ConvertPrice(tourDto.Price, tourDto.Currency ?? "TRY", selectedCurrency);

        var model = new TourBookingViewModel
        {
            TourId = (int)(tourDto.Id.GetHashCode() & 0x7FFFFFFF),
            TourRawId = tourDto.Id,
            TourName = tourDto.Name,
            Destination = tourDto.Destination,
            Duration = tourDto.Duration,
            TourDate = tourDate ?? DateTime.Now.AddDays(7),
            ParticipantCount = participantCount,
            Price = priceInSelectedCurrency,
            Currency = selectedCurrency,
            Passengers = passengers
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiatePayment(TourBookingViewModel model, CancellationToken ct = default)
    {
        // Derive TourId from TourRawId when not posted (avoids "value is not valid for TourId" when Guid was sent)
        if (model.TourRawId != Guid.Empty && model.TourId == 0)
            model.TourId = (int)(model.TourRawId.GetHashCode() & 0x7FFFFFFF);
        ModelState.Remove("TourId");

        if (model.TourRawId == Guid.Empty)
        {
            TempData["BookingError"] = "Tour information is missing. Please select a tour again from the tours page.";
            return RedirectToAction(nameof(Packages));
        }

        // Minimal validation (Flight-style): at least one passenger with required contact fields
        var passengerCount = model.ParticipantCount > 0 ? model.ParticipantCount : 1;
        if (model.Passengers == null || model.Passengers.Count < passengerCount)
        {
            EnsurePassengersCount(model);
            EnsurePassengersCountForDisplay(model);
            TempData["BookingError"] = "Enter at least one passenger information.";
            return View("Booking", model);
        }
        var firstPassenger = model.Passengers.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstPassenger?.FirstName) || string.IsNullOrWhiteSpace(firstPassenger?.LastName) ||
            string.IsNullOrWhiteSpace(firstPassenger?.Email))
        {
            EnsurePassengersCount(model);
            EnsurePassengersCountForDisplay(model);
            TempData["BookingError"] = "First name, last name and email are required for the first passenger.";
            return View("Booking", model);
        }

        // Check if user is authenticated (either as regular user or admin)
        var isAuthenticated = _authService.IsAuthenticated() || User.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
        {
            TempData["BookingError"] = "You must log in to make a reservation.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Booking), "Tour", new { tourId = model.TourRawId }) });
        }

        var (success, message, tourDto) = await _tourService.GetByIdAsync(model.TourRawId, ct);
        if (!success || tourDto == null)
        {
            TempData["BookingError"] = "Tour not found: " + message;
            return RedirectToAction(nameof(Packages));
        }

        // Store booking in Session (like Flight) so Payment page can read it
        var json = System.Text.Json.JsonSerializer.Serialize(model);
        HttpContext.Session.SetString("TourPendingBooking", json);
        return RedirectToAction(nameof(Payment));
    }

    [HttpGet]
    public async Task<IActionResult> Payment(CancellationToken ct = default)
    {
        var json = HttpContext.Session.GetString("TourPendingBooking");
        if (string.IsNullOrEmpty(json))
        {
            TempData["ErrorMessage"] = "Please fill in reservation information before going to the payment page.";
            return RedirectToAction(nameof(Packages));
        }
        TourBookingViewModel? booking;
        try
        {
            booking = System.Text.Json.JsonSerializer.Deserialize<TourBookingViewModel>(json);
        }
        catch
        {
            TempData["ErrorMessage"] = "Rezervasyon verisi okunamadi. Lutfen tekrar deneyin.";
            return RedirectToAction(nameof(Packages));
        }
        if (booking == null || booking.TourRawId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Reservation information is incomplete. Please make the reservation again.";
            return RedirectToAction(nameof(Packages));
        }
        var (success, message, tourDto) = await _tourService.GetByIdAsync(booking.TourRawId, ct);
        if (!success || tourDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Packages));
        }
        var firstPassenger = booking.Passengers?.FirstOrDefault();
        var model = new TourPaymentViewModel
        {
            TourId = booking.TourId != 0 ? booking.TourId : (int)(tourDto.Id.GetHashCode() & 0x7FFFFFFF),
            TourRawId = booking.TourRawId,
            TourName = booking.TourName ?? tourDto.Name,
            Destination = booking.Destination ?? tourDto.Destination,
            Duration = booking.Duration != 0 ? booking.Duration : tourDto.Duration,
            TourDate = booking.TourDate != default ? booking.TourDate : DateTime.Now.AddDays(7),
            ParticipantCount = booking.ParticipantCount > 0 ? booking.ParticipantCount : 1,
            Price = booking.Price > 0 ? booking.Price : tourDto.Price,
            Currency = booking.Currency ?? "TRY",
            ContactName = firstPassenger != null ? $"{firstPassenger.FirstName} {firstPassenger.LastName}".Trim() : "",
            ContactEmail = firstPassenger?.Email ?? "",
            ContactPhone = firstPassenger?.Phone ?? ""
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletePayment(TourPaymentViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return View("Payment", model);
        }
        var isAuthenticated = _authService.IsAuthenticated() || User.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
        {
            TempData["BookingError"] = "You must log in to make a reservation.";
            return RedirectToAction("Login", "Account");
        }
        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["BookingError"] = "Could not retrieve user information.";
            return RedirectToAction(nameof(Packages));
        }
        
        // Load booking from session to get Passengers
        var bookingJson = HttpContext.Session.GetString("TourPendingBooking");
        TourBookingViewModel? booking = null;
        if (!string.IsNullOrEmpty(bookingJson))
        {
            try
            {
                booking = System.Text.Json.JsonSerializer.Deserialize<TourBookingViewModel>(bookingJson);
            }
            catch
            {
                // If deserialization fails, continue without passengers
            }
        }
        
        // Get tour to know its original currency
        var (tourSuccess, tourMessage, tourDto) = await _tourService.GetByIdAsync(model.TourRawId, ct);
        if (!tourSuccess || tourDto == null)
        {
            TempData["BookingError"] = "Tur bilgisi alinamadi.";
            return View("Payment", model);
        }
        
        // Get selected currency from cookie
        var selectedCurrency = _cookieHelper.GetCurrency();
        
        // model.Total already includes subtotal + 10% tax (TourPaymentViewModel)
        decimal finalTotal = model.Total;
        
        if (!Enum.TryParse<Currency>(selectedCurrency, true, out var currency))
            currency = Currency.TRY;
        
        var payment = new CreatePaymentDto
        {
            TransactionAmount = finalTotal,
            Currency = currency,
            PaymentMethod = ParsePaymentMethod(model.PaymentMethod),
            TransactionId = GenerateTransactionId(),
            TransactionType = TransactionType.Payment
        };
        var createReservationDto = new CreateReservationDto
        {
            AppUserId = userId,
            TotalPrice = finalTotal,
            Currency = currency,
            Type = ReservationType.Tour,
            TourId = model.TourRawId,
            PNR = GeneratePNR(),
            Tickets = new List<CreateTicketDto>(),
            Participants = booking?.Passengers?.Where(p => !string.IsNullOrWhiteSpace(p.FirstName))
                .Select(p => new TravelBooking.Web.DTOs.Passengers.CreatePassengerDto
                {
                    PassengerFirstName = p.FirstName ?? "",
                    PassengerLastName = p.LastName ?? "",
                    NationalNumber = "",
                    PassportNumber = "",
                    DateOfBirth = p.DateOfBirth ?? DateTime.Now.AddYears(-30),
                    PassengerType = "Adult"
                }).ToList() ?? new List<TravelBooking.Web.DTOs.Passengers.CreatePassengerDto>(),
            Payment = payment
        };
        var (reservationSuccess, reservationMessage, reservationId) = await _reservationService.CreateAsync(createReservationDto, ct);
        if (!reservationSuccess || !reservationId.HasValue)
        {
            TempData["BookingError"] = $"Reservation could not be created: {reservationMessage}";
            return View("Payment", model);
        }
        HttpContext.Session.Remove("TourPendingBooking");
        TempData["BookingSuccess"] = $"Your tour reservation was created successfully! PNR: {createReservationDto.PNR}";
        return RedirectToAction("MyReservations", "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteBooking(TourBookingViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            EnsurePassengersCount(model);
            EnsurePassengersCountForDisplay(model);
            return View("Booking", model);
        }

        // Check if user is authenticated (either as regular user or admin)
        var isAuthenticatedForReservation = _authService.IsAuthenticated() || User.Identity?.IsAuthenticated == true;
        if (!isAuthenticatedForReservation)
        {
            TempData["BookingError"] = "You must log in to make a reservation.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Booking), "Tour") });
        }

        try
        {
            // Get user ID from auth service
            var userId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                TempData["BookingError"] = "Could not retrieve user information.";
                return RedirectToAction(nameof(Packages));
            }

            // Total: Price * ParticipantCount + 10% tax (TourBookingViewModel has Price, ParticipantCount)
            decimal totalPrice = model.Price * model.ParticipantCount;
            decimal taxAmount = totalPrice * 0.1m;
            decimal finalTotal = totalPrice + taxAmount;

            // Parse currency
            if (!Enum.TryParse<Currency>(model.Currency, true, out var currency))
            {
                currency = Currency.USD;
            }

            // Parse payment method
            PaymentMethod paymentMethod = ParsePaymentMethod(model.PaymentMethod);

            // Create payment
            var payment = new CreatePaymentDto
            {
                TransactionAmount = finalTotal,
                Currency = currency,
                PaymentMethod = paymentMethod,
                TransactionId = GenerateTransactionId(),
                TransactionType = TransactionType.Payment
            };

            // Create reservation (without tickets, only payment; include participants)
            var createReservationDto = new CreateReservationDto
            {
                AppUserId = userId,
                TotalPrice = finalTotal,
                Currency = currency,
                Type = ReservationType.Tour,
                TourId = model.TourRawId,
                PNR = GeneratePNR(),
                Tickets = new List<CreateTicketDto>(), // No tickets for tour bookings
                Participants = model.Passengers?.Where(p => !string.IsNullOrWhiteSpace(p.FirstName))
                    .Select(p => new TravelBooking.Web.DTOs.Passengers.CreatePassengerDto
                    {
                        PassengerFirstName = p.FirstName,
                        PassengerLastName = p.LastName,
                        NationalNumber = "",
                        PassportNumber = "",
                        DateOfBirth = p.DateOfBirth ?? DateTime.UtcNow.AddYears(-25),
                        PassengerType = "Adult"
                    }).ToList() ?? new List<TravelBooking.Web.DTOs.Passengers.CreatePassengerDto>(),
                Payment = payment
            };

            var (reservationSuccess, reservationMessage, reservationId) = await _reservationService.CreateAsync(createReservationDto, ct);
            
            if (!reservationSuccess || !reservationId.HasValue)
            {
                TempData["BookingError"] = $"Reservation could not be created: {reservationMessage}";
                EnsurePassengersCount(model);
                EnsurePassengersCountForDisplay(model);
                return View("Booking", model);
            }

            TempData["BookingSuccess"] = $"Your tour reservation was created successfully! PNR: {createReservationDto.PNR}";
            return RedirectToAction("MyReservations", "Account");
        }
        catch (Exception ex)
        {
            TempData["BookingError"] = $"Bir hata olustu: {ex.Message}";
            EnsurePassengersCount(model);
            EnsurePassengersCountForDisplay(model);
            return View("Booking", model);
        }
    }

    private PaymentMethod ParsePaymentMethod(string method)
    {
        return method?.ToLower() switch
        {
            "card" => PaymentMethod.Card,
            "cash" => PaymentMethod.Cash,
            "paypal" => PaymentMethod.PayPal,
            "banktransfer" => PaymentMethod.BankTransfer,
            _ => PaymentMethod.Card
        };
    }

    private string GenerateTransactionId()
    {
        return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }

    private string GeneratePNR()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static void EnsurePassengersCount(TourBookingViewModel model)
    {
        var count = Math.Clamp(model.ParticipantCount, 1, 20);
        model.Passengers ??= new List<TourPassengerViewModel>();
        while (model.Passengers.Count < count)
            model.Passengers.Add(new TourPassengerViewModel());
        if (model.Passengers.Count > count)
            model.Passengers = model.Passengers.Take(count).ToList();
    }

    /// <summary>Pads Passengers to 20 so the view can render all blocks for dynamic show/hide.</summary>
    private static void EnsurePassengersCountForDisplay(TourBookingViewModel model)
    {
        model.Passengers ??= new List<TourPassengerViewModel>();
        while (model.Passengers.Count < 20)
            model.Passengers.Add(new TourPassengerViewModel());
    }
}
