using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels.Cars;
using TravelBooking.Web.ViewModels.Payments;
using TravelBooking.Web.Services.Cars;
using TravelBooking.Web.Services.Reservations;
using TravelBooking.Web.Services.Auth;
using TravelBooking.Web.Services.Currency;
using TravelBooking.Web.Helpers;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.Controllers;

public class CarController : Controller
{
    private readonly ICarService _carService;
    private readonly IReservationService _reservationService;
    private readonly IAuthService _authService;
    private readonly ICookieHelper _cookieHelper;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CarController> _logger;

    public CarController(
        ICarService carService,
        IReservationService reservationService,
        IAuthService authService,
        ICookieHelper cookieHelper,
        ICurrencyService currencyService,
        ILogger<CarController> logger)
    {
        _carService = carService;
        _reservationService = reservationService;
        _authService = authService;
        _cookieHelper = cookieHelper;
        _currencyService = currencyService;
        _logger = logger;
    }

    public async Task<IActionResult> Listing(string? location, DateTime? pickupDate, DateTime? returnDate, string? category, CancellationToken ct = default)
    {
        var (success, message, cars) = string.IsNullOrWhiteSpace(location) && string.IsNullOrWhiteSpace(category)
            ? await _carService.GetAllAsync(ct)
            : await _carService.SearchAsync(location, category, null, ct);

        var carViewModels = cars.Select(c => new CarViewModel
        {
            Id = (int)(c.Id.GetHashCode() & 0x7FFFFFFF),
            RawId = c.Id,
            Brand = c.Brand,
            Model = c.Model,
            Category = c.Category,
            Year = c.Year,
            FuelType = c.FuelType,
            Transmission = c.Transmission,
            Seats = c.Seats,
            Doors = c.Doors,
            PricePerDay = c.PricePerDay,
            Currency = c.Currency,
            ImageUrl = c.ImageUrl,
            Location = c.Location,
            HasAirConditioning = c.HasAirConditioning,
            HasGPS = c.HasGPS,
            Rating = c.Rating,
            ReviewCount = c.ReviewCount
        }).ToList();

        var model = new CarListingViewModel
        {
            Cars = carViewModels,
            SearchLocation = location,
            PickupDate = pickupDate,
            ReturnDate = returnDate,
            Category = category
        };

        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }

        return View(model);
    }

    public async Task<IActionResult> Detail(Guid id, DateTime? pickupDate, DateTime? returnDate, string? pickupLocation, CancellationToken ct = default)
    {
        var (success, message, carDto) = await _carService.GetByIdAsync(id, ct);
        
        if (!success || carDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }

        var car = new CarViewModel
        {
            Id = (int)(carDto.Id.GetHashCode() & 0x7FFFFFFF),
            RawId = carDto.Id,
            Brand = carDto.Brand,
            Model = carDto.Model,
            Category = carDto.Category,
            Year = carDto.Year,
            FuelType = carDto.FuelType,
            Transmission = carDto.Transmission,
            Seats = carDto.Seats,
            Doors = carDto.Doors,
            PricePerDay = carDto.PricePerDay,
            Currency = carDto.Currency,
            ImageUrl = carDto.ImageUrl,
            Location = carDto.Location,
            HasAirConditioning = carDto.HasAirConditioning,
            HasGPS = carDto.HasGPS,
            Rating = carDto.Rating,
            ReviewCount = carDto.ReviewCount
        };

        var model = new CarDetailViewModel
        {
            Car = car,
            Features = new List<string>(),
            IncludedInPrice = new List<string>(),
            Reviews = new List<CarReviewViewModel>(),
            PickupDate = pickupDate ?? DateTime.Now.AddDays(1),
            ReturnDate = returnDate ?? DateTime.Now.AddDays(3),
            PickupLocation = pickupLocation ?? carDto.Location,
            ReturnLocation = pickupLocation ?? carDto.Location
        };

        return View(model);
    }

    public async Task<IActionResult> Booking(Guid carId, DateTime? pickupDate, DateTime? returnDate, string? pickupLocation, CancellationToken ct = default)
    {
        _logger.LogInformation("Booking GET: carId={CarId}, pickupDate={PickupDate}, returnDate={ReturnDate}", carId, pickupDate, returnDate);
        
        var (success, message, carDto) = await _carService.GetByIdAsync(carId, ct);
        
        if (!success || carDto == null)
        {
            _logger.LogWarning("Booking GET: Car not found for carId={CarId}", carId);
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }

        _logger.LogInformation("Booking GET: carDto - Brand={Brand}, Model={Model}, Location={Location}", 
            carDto.Brand, carDto.Model, carDto.Location);

        var days = returnDate.HasValue && pickupDate.HasValue 
            ? (int)(returnDate.Value - pickupDate.Value).TotalDays 
            : 1;
        if (days < 1) days = 1;

        // Get selected currency from cookie
        var selectedCurrency = _cookieHelper.GetCurrency();
        var subtotalInCarCurrency = carDto.PricePerDay * days;
        var totalPriceInSelectedCurrency = _currencyService.ConvertPrice(subtotalInCarCurrency, carDto.Currency ?? "TRY", selectedCurrency);

        var model = new CarBookingViewModel
        {
            CarId = (int)(carDto.Id.GetHashCode() & 0x7FFFFFFF),
            RawCarId = carDto.Id,
            Brand = carDto.Brand,
            Model = carDto.Model,
            ImageUrl = carDto.ImageUrl,
            PickupDate = pickupDate ?? DateTime.Now.AddDays(1),
            ReturnDate = returnDate ?? DateTime.Now.AddDays(2),
            PickupLocation = pickupLocation ?? carDto.Location,
            ReturnLocation = pickupLocation ?? carDto.Location,
            TotalPrice = totalPriceInSelectedCurrency,
            Currency = selectedCurrency
        };

        _logger.LogInformation("Booking GET: Returning view with RawCarId={RawCarId}, Brand={Brand}, Model={Model}, PickupLocation={PickupLocation}, ReturnLocation={ReturnLocation}", 
            model.RawCarId, model.Brand, model.Model, model.PickupLocation, model.ReturnLocation);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiatePayment(CarBookingViewModel model, Guid? carId, CancellationToken ct = default)
    {
        // Set RawCarId from route if not in model
        if (model.RawCarId == Guid.Empty && carId.HasValue)
            model.RawCarId = carId.Value;

        if (model.RawCarId == Guid.Empty)
        {
            TempData["BookingError"] = "Vehicle information is missing. Please select again from the car list.";
            return RedirectToAction(nameof(Listing));
        }

        // Get car details from API to fill missing data
        var (carSuccess, carMessage, carDetails) = await _carService.GetByIdAsync(model.RawCarId, ct);
        if (!carSuccess || carDetails == null)
        {
            TempData["ErrorMessage"] = "Car not found.";
            return RedirectToAction(nameof(Listing));
        }

        // Fill model with car data
        model.CarId = (int)(carDetails.Id.GetHashCode() & 0x7FFFFFFF);
        model.Brand = carDetails.Brand;
        model.Model = carDetails.Model;
        model.ImageUrl = carDetails.ImageUrl;
        
        // Keep user's pickup/return location if provided, otherwise use car location
        if (string.IsNullOrWhiteSpace(model.PickupLocation))
            model.PickupLocation = carDetails.Location;
        if (string.IsNullOrWhiteSpace(model.ReturnLocation))
            model.ReturnLocation = carDetails.Location;

        // Validate user input fields
        if (!ModelState.IsValid || !model.AcceptTerms)
        {
            // Recalculate price for error display
            var daysCount = (int)(model.ReturnDate - model.PickupDate).TotalDays;
            if (daysCount < 1) daysCount = 1;
            var selectedCurrency = _cookieHelper.GetCurrency();
            model.TotalPrice = _currencyService.ConvertPrice(carDetails.PricePerDay * daysCount, carDetails.Currency ?? "TRY", selectedCurrency);
            model.Currency = selectedCurrency;
            
            if (!model.AcceptTerms)
                TempData["BookingError"] = "You must accept the Terms of Use and Privacy Policy to proceed to the payment screen.";
            
            return View("Booking", model);
        }

        // All validation passed, calculate final price
        var rentalDays = (int)(model.ReturnDate - model.PickupDate).TotalDays;
        if (rentalDays < 1) rentalDays = 1;
        
        var userCurrency = _cookieHelper.GetCurrency();
        var totalPriceInUserCurrency = _currencyService.ConvertPrice(carDetails.PricePerDay * rentalDays, carDetails.Currency ?? "TRY", userCurrency);

        // Calculate extras prices (base prices in TRY)
        var kaskoPrice = model.HasKasko ? _currencyService.ConvertPrice(5000m, "TRY", userCurrency) : 0m;
        var driverPricePerDay = model.HasAdditionalDriver ? _currencyService.ConvertPrice(500m, "TRY", userCurrency) : 0m;
        var childSeatPricePerDay = _currencyService.ConvertPrice(200m, "TRY", userCurrency);
        var boosterSeatPricePerDay = _currencyService.ConvertPrice(150m, "TRY", userCurrency);

        var paymentVm = new CarPaymentViewModel
        {
            CarId = model.RawCarId,
            CarModel = $"{model.Brand} {model.Model}",
            Category = carDetails.Category,
            PickUpLocation = model.PickupLocation,
            DropOffLocation = model.ReturnLocation,
            PickUpDate = model.PickupDate,
            DropOffDate = model.ReturnDate,
            Days = rentalDays,
            PricePerDay = _currencyService.ConvertPrice(carDetails.PricePerDay, carDetails.Currency ?? "TRY", userCurrency),
            Currency = userCurrency,
            Transmission = carDetails.Transmission,
            Seats = carDetails.Seats,
            RenterName = $"{model.FirstName} {model.LastName}".Trim(),
            RenterEmail = model.ContactEmail,
            RenterPhone = model.ContactPhone,
            // Extras
            HasKasko = model.HasKasko,
            KaskoPrice = kaskoPrice,
            HasAdditionalDriver = model.HasAdditionalDriver,
            DriverPrice = driverPricePerDay * rentalDays,
            ChildSeatCount = model.ChildSeatCount,
            ChildSeatPrice = childSeatPricePerDay,
            BoosterSeatCount = model.BoosterSeatCount,
            BoosterSeatPrice = boosterSeatPricePerDay
        };

        var json = System.Text.Json.JsonSerializer.Serialize(model);
        HttpContext.Session.SetString("CarPendingBooking", json);

        return View("Payment", paymentVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletePayment(CarPaymentViewModel paymentForm, CancellationToken ct = default)
    {
        var pendingJson = HttpContext.Session.GetString("CarPendingBooking");
        if (string.IsNullOrEmpty(pendingJson))
        {
            TempData["BookingError"] = "Reservation session not found. Please enter your information again.";
            return RedirectToAction(nameof(Listing));
        }

        CarBookingViewModel? model;
        try
        {
            model = System.Text.Json.JsonSerializer.Deserialize<CarBookingViewModel>(pendingJson);
        }
        catch
        {
            TempData["BookingError"] = "Could not read reservation data.";
            return RedirectToAction(nameof(Listing));
        }

        if (model == null)
        {
            TempData["BookingError"] = "Reservation data not found.";
            return RedirectToAction(nameof(Listing));
        }

        // Payment method kontrolu (Card secilmisse kart alanlari zorunlu)
        if (string.Equals(paymentForm.PaymentMethod, "Card", StringComparison.OrdinalIgnoreCase) 
            && string.IsNullOrWhiteSpace(paymentForm.CardNumber))
        {
            TempData["BookingError"] = "Please enter payment information.";
            return View("Payment", paymentForm);
        }

        // Check if user is authenticated (either as regular user or admin)
        var isAuthenticated = _authService.IsAuthenticated() || User.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
        {
            TempData["BookingError"] = "You must log in to make a reservation.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Booking), "Car", new { carId = model.RawCarId }) });
        }

        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["BookingError"] = "Could not retrieve user information.";
            return RedirectToAction(nameof(Listing));
        }

        // Get car to know its original currency
        var (carSuccess, carMessage, carDto) = await _carService.GetByIdAsync(model.RawCarId, ct);
        if (!carSuccess || carDto == null)
        {
            TempData["BookingError"] = "Could not retrieve car information.";
            return View("Payment", paymentForm);
        }

        // Get selected currency from cookie
        var selectedCurrency = _cookieHelper.GetCurrency();

        // paymentForm.Total already includes subtotal + extras + tax (calculated property)
        var finalTotal = paymentForm.Total;

        if (!Enum.TryParse<Currency>(selectedCurrency, true, out var currency))
            currency = Currency.TRY;

        var payment = new CreatePaymentDto
        {
            TransactionAmount = finalTotal,
            Currency = currency,
            PaymentMethod = ParsePaymentMethod(paymentForm.PaymentMethod),
            TransactionId = GenerateTransactionId(),
            TransactionType = TransactionType.Payment
        };

        // Katilimci: arac kiralayan (rezervasyon detayda bilet gibi gorunsun) - paymentForm'dan al
        var participants = new List<TravelBooking.Web.DTOs.Passengers.CreatePassengerDto>();
        var renterName = paymentForm.RenterName ?? $"{model.FirstName} {model.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(renterName))
        {
            var nameParts = renterName.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : renterName.Trim();
            var lastName = nameParts.Length > 1 ? nameParts[1] : "-";
            participants.Add(new TravelBooking.Web.DTOs.Passengers.CreatePassengerDto
            {
                PassengerFirstName = firstName,
                PassengerLastName = lastName,
                DateOfBirth = new DateTime(1990, 1, 1),
                PassengerType = "Adult"
            });
        }

        var createReservationDto = new CreateReservationDto
        {
            AppUserId = userId,
            TotalPrice = finalTotal,
            Currency = currency,
            Type = ReservationType.Car,
            CarId = model.RawCarId,
            PNR = GeneratePNR(),
            Tickets = new List<CreateTicketDto>(),
            Participants = participants,
            Payment = payment,
            CarPickUpDate = paymentForm.PickUpDate,
            CarDropOffDate = paymentForm.DropOffDate,
            CarPickUpLocation = paymentForm.PickUpLocation,
            CarDropOffLocation = paymentForm.DropOffLocation
        };

        var (reservationSuccess, reservationMessage, reservationId) = await _reservationService.CreateAsync(createReservationDto, ct);
        
        if (!reservationSuccess || !reservationId.HasValue)
        {
            TempData["BookingError"] = $"Reservation could not be created: {reservationMessage}";
            return View("Payment", paymentForm);
        }

        HttpContext.Session.Remove("CarPendingBooking");
        TempData["BookingSuccess"] = $"Your car rental reservation was created successfully! PNR: {createReservationDto.PNR}";
        return RedirectToAction("MyReservations", "Account");
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
}
