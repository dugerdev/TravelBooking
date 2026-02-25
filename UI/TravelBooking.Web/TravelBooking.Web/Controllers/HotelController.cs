using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels.Hotels;
using TravelBooking.Web.Services.Hotels;
using TravelBooking.Web.Services.Reservations;
using TravelBooking.Web.Services.Auth;
using TravelBooking.Web.Services.Currency;
using TravelBooking.Web.Helpers;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.Controllers;

public class HotelController : Controller
{
    private readonly IHotelService _hotelService;
    private readonly IReservationService _reservationService;
    private readonly IAuthService _authService;
    private readonly ICookieHelper _cookieHelper;
    private readonly ICurrencyService _currencyService;

    public HotelController(
        IHotelService hotelService,
        IReservationService reservationService,
        IAuthService authService,
        ICookieHelper cookieHelper,
        ICurrencyService currencyService)
    {
        _hotelService = hotelService;
        _reservationService = reservationService;
        _authService = authService;
        _cookieHelper = cookieHelper;
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Listing(string? city, int? minStarRating, decimal? maxPricePerNight, CancellationToken ct = default)
    {
        var (success, message, hotels) = string.IsNullOrWhiteSpace(city) && !minStarRating.HasValue && !maxPricePerNight.HasValue
            ? await _hotelService.GetAllAsync(ct)
            : await _hotelService.SearchAsync(city, minStarRating, maxPricePerNight, ct);

        var hotelViewModels = hotels.Select(h => {
            //Örnek: Standart oda fiyatini kullan; yoksa otel genel fiyatini goster
            var standardRoom = h.Rooms?.FirstOrDefault(r => r.Type == "Standart Oda" || r.Type?.Contains("Standard", StringComparison.OrdinalIgnoreCase) == true);
            var displayPrice = standardRoom?.Price ?? h.PricePerNight;
            var displayCurrency = standardRoom?.Currency ?? h.Currency;
            
            return new HotelViewModel
            {
                //Örnek: Guid'i pozitif int'e donusturur - View'daki dropdown/select icin (Guid binding sorunlarini onler)
                Id = (int)(h.Id.GetHashCode() & 0x7FFFFFFF),
                RawId = h.Id,
                Name = h.Name,
                City = h.City,
                Country = h.Country,
                Address = h.Address,
                StarRating = h.StarRating,
                PricePerNight = displayPrice,
                Currency = displayCurrency,
                ImageUrl = h.ImageUrl,
                Description = h.Description,
                Rating = h.Rating,
                ReviewCount = h.ReviewCount,
                HasFreeWifi = h.HasFreeWifi,
                HasParking = h.HasParking,
                HasPool = h.HasPool,
                HasRestaurant = h.HasRestaurant
            };
        }).ToList();

        var model = new HotelListingViewModel
        {
            Hotels = hotelViewModels,
            SearchCity = city,
            MinStarRating = minStarRating,
            MaxPricePerNight = maxPricePerNight
        };

        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }

        return View(model);
    }

    public async Task<IActionResult> Detail(Guid id, CancellationToken ct = default)
    {
        var (success, message, hotelDto) = await _hotelService.GetByIdAsync(id, ct);
        
        if (!success || hotelDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }

        var hotel = new HotelViewModel
        {
            Id = (int)(hotelDto.Id.GetHashCode() & 0x7FFFFFFF),
            RawId = hotelDto.Id,
            Name = hotelDto.Name,
            City = hotelDto.City,
            Country = hotelDto.Country,
            Address = hotelDto.Address,
            StarRating = hotelDto.StarRating,
            PricePerNight = hotelDto.PricePerNight,
            Currency = hotelDto.Currency,
            ImageUrl = hotelDto.ImageUrl,
            Description = hotelDto.Description,
            Rating = hotelDto.Rating,
            ReviewCount = hotelDto.ReviewCount,
            HasFreeWifi = hotelDto.HasFreeWifi,
            HasParking = hotelDto.HasParking,
            HasPool = hotelDto.HasPool,
            HasRestaurant = hotelDto.HasRestaurant
        };

        var rooms = hotelDto.Rooms?.Select(r => new RoomViewModel
        {
            Id = (int)(r.Id.GetHashCode() & 0x7FFFFFFF),
            Type = r.Type,
            Price = r.Price,
            Currency = r.Currency ?? hotelDto.Currency,
            MaxGuests = r.MaxGuests,
            Description = r.Description,
            Features = r.Features
        }).ToList() ?? new List<RoomViewModel>();

        var model = new HotelDetailViewModel
        {
            Hotel = hotel,
            Rooms = rooms,
            Reviews = new List<HotelReviewViewModel>()
        };

        return View(model);
    }

    public async Task<IActionResult> Booking(Guid hotelId, Guid roomId, DateTime? checkIn, DateTime? checkOut, int guests = 1, CancellationToken ct = default)
    {
        var (success, message, hotelDto) = await _hotelService.GetByIdAsync(hotelId, ct);
        
        if (!success || hotelDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }

        var room = hotelDto.Rooms?.FirstOrDefault(r => r.Id == roomId);
        if (room == null && hotelDto.Rooms?.Any() == true)
        {
            room = hotelDto.Rooms.First();
        }

        if (room == null)
        {
            TempData["ErrorMessage"] = "Room not found.";
            return RedirectToAction(nameof(Detail), new { id = hotelId });
        }
        
        var nights = checkOut.HasValue && checkIn.HasValue 
            ? (int)(checkOut.Value - checkIn.Value).TotalDays 
            : 1;

        // Get selected currency from cookie
        var selectedCurrency = _cookieHelper.GetCurrency();
        var subtotalInRoomCurrency = room.Price * nights;
        var totalPriceInSelectedCurrency = _currencyService.ConvertPrice(subtotalInRoomCurrency, room.Currency ?? "TRY", selectedCurrency);

        var model = new HotelBookingViewModel
        {
            HotelId = (int)(hotelDto.Id.GetHashCode() & 0x7FFFFFFF),
            RawHotelId = hotelDto.Id,
            RoomId = (int)(room.Id.GetHashCode() & 0x7FFFFFFF),
            HotelName = hotelDto.Name,
            RoomType = room.Type,
            CheckIn = checkIn ?? DateTime.Now.AddDays(1),
            CheckOut = checkOut ?? DateTime.Now.AddDays(2),
            Guests = guests,
            TotalPrice = totalPriceInSelectedCurrency,
            Currency = selectedCurrency
        };

        return View(model);
    }

    /// <summary>Detail sayfasindan gelen form: oda, tarih, misafir ve iletisim bilgilerini alir, TotalPrice'i hesaplar, session'a yazar ve odeme sayfasina yonlendirir.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiatePaymentFromDetail(
        Guid RawHotelId,
        string HotelName,
        int RoomId,
        string RoomType,
        DateTime CheckIn,
        DateTime CheckOut,
        int Guests,
        string ContactName,
        string ContactEmail,
        string? ContactPhone,
        string? IdOrPassport,
        CancellationToken ct = default)
    {
        if (RawHotelId == Guid.Empty)
        {
            TempData["BookingError"] = "Hotel information is missing.";
            return RedirectToAction(nameof(Listing));
        }
        
        // Validation: Contact bilgileri
        if (string.IsNullOrWhiteSpace(ContactName) || string.IsNullOrWhiteSpace(ContactEmail))
        {
            TempData["BookingError"] = "Full name and email are required.";
            return RedirectToAction(nameof(Detail), new { id = RawHotelId });
        }
        
        // Validation: Email format
        if (!ContactEmail.Contains("@") || !ContactEmail.Contains("."))
        {
            TempData["BookingError"] = "Please enter a valid email address.";
            return RedirectToAction(nameof(Detail), new { id = RawHotelId });
        }
        
        // Validation: Tarihler
        if (CheckIn < DateTime.Today)
        {
            TempData["BookingError"] = "Check-in date cannot be in the past.";
            return RedirectToAction(nameof(Detail), new { id = RawHotelId });
        }
        
        if (CheckOut <= CheckIn)
        {
            TempData["BookingError"] = "Check-out date must be after check-in date.";
            return RedirectToAction(nameof(Detail), new { id = RawHotelId });
        }
        var (success, message, hotelDto) = await _hotelService.GetByIdAsync(RawHotelId, ct);
        if (!success || hotelDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }
        //Örnek: RoomId form'dan int olarak gelir; Guid ile eslestirmek icin GetHashCode kullanilir
        var room = hotelDto.Rooms?.FirstOrDefault(r => (r.Id.GetHashCode() & 0x7FFFFFFF) == RoomId);
        if (room == null && hotelDto.Rooms?.Any() == true)
            room = hotelDto.Rooms.First();
        if (room == null)
        {
            TempData["BookingError"] = "Room not found.";
            return RedirectToAction(nameof(Detail), new { id = RawHotelId });
        }
        
        //Örnek: Oda musaitlik kontrolu
        if (!room.IsAvailable)
        {
            TempData["BookingError"] = "The selected room is not currently available.";
            return RedirectToAction(nameof(Detail), new { id = RawHotelId });
        }
        
        // Validation: Misafir sayisi kontrolu
        if (Guests > room.MaxGuests)
        {
            TempData["BookingError"] = $"This room can accommodate a maximum of {room.MaxGuests} guests.";
            return RedirectToAction(nameof(Detail), new { id = RawHotelId });
        }
        
        var nights = (int)(CheckOut - CheckIn).TotalDays;
        if (nights < 1) nights = 1;
        var selectedCurrency = _cookieHelper.GetCurrency();
        var subtotalInRoomCurrency = room.Price * nights;
        var totalInSelectedCurrency = _currencyService.ConvertPrice(subtotalInRoomCurrency, room.Currency ?? hotelDto.Currency ?? "TRY", selectedCurrency);
        var model = new HotelBookingViewModel
        {
            HotelId = (int)(hotelDto.Id.GetHashCode() & 0x7FFFFFFF),
            RawHotelId = hotelDto.Id,
            RoomId = (int)(room.Id.GetHashCode() & 0x7FFFFFFF),
            HotelName = hotelDto.Name,
            RoomType = room.Type,
            CheckIn = CheckIn,
            CheckOut = CheckOut,
            Guests = Math.Clamp(Guests, 1, 10),
            TotalPrice = totalInSelectedCurrency,
            Currency = selectedCurrency,
            ContactName = ContactName.Trim(),
            ContactEmail = ContactEmail.Trim(),
            ContactPhone = ContactPhone?.Trim(),
            IdOrPassport = IdOrPassport?.Trim()
        };
        var json = System.Text.Json.JsonSerializer.Serialize(model);
        HttpContext.Session.SetString("HotelPendingBooking", json);
        return RedirectToAction(nameof(Payment));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiatePayment(HotelBookingViewModel model, CancellationToken ct = default)
    {
        if (model.RawHotelId == Guid.Empty)
        {
            TempData["BookingError"] = "Hotel information is missing. Please select again from the hotel list.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: Form'dan HotelId gelmemisse RawHotelId'den turetilir; Guid-int binding hatasini onler
        if (model.HotelId == 0)
            model.HotelId = (int)(model.RawHotelId.GetHashCode() & 0x7FFFFFFF);
        ModelState.Remove("HotelId");
        ModelState.Remove("RoomId");

        if (string.IsNullOrEmpty(model.HotelName) || model.TotalPrice <= 0)
        {
            TempData["BookingError"] = "Invalid reservation information.";
            return RedirectToAction(nameof(Listing));
        }

        var (success, message, hotelDto) = await _hotelService.GetByIdAsync(model.RawHotelId, ct);
        
        if (!success || hotelDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }

        if (model.RoomId == 0 && hotelDto.Rooms?.Any() == true)
            model.RoomId = (int)(hotelDto.Rooms.First().Id.GetHashCode() & 0x7FFFFFFF);

        // Minimal validation: contact fields required
        if (string.IsNullOrWhiteSpace(model.ContactName) || string.IsNullOrWhiteSpace(model.ContactEmail))
        {
            TempData["BookingError"] = "Contact name and email are required.";
            return View("Booking", model);
        }

        // Store booking in Session (like Tour/Flight) so Payment page can read it
        var json = System.Text.Json.JsonSerializer.Serialize(model);
        HttpContext.Session.SetString("HotelPendingBooking", json);
        return RedirectToAction(nameof(Payment));
    }

    [HttpGet]
    public async Task<IActionResult> Payment(CancellationToken ct = default)
    {
        var json = HttpContext.Session.GetString("HotelPendingBooking");
        if (string.IsNullOrEmpty(json))
        {
            TempData["ErrorMessage"] = "Please fill in reservation information before going to the payment page.";
            return RedirectToAction(nameof(Listing));
        }
        HotelBookingViewModel? booking;
        try
        {
            booking = System.Text.Json.JsonSerializer.Deserialize<HotelBookingViewModel>(json);
        }
        catch
        {
            TempData["ErrorMessage"] = "Could not read reservation data. Please try again.";
            return RedirectToAction(nameof(Listing));
        }
        if (booking == null || booking.RawHotelId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Reservation information is incomplete. Please make the reservation again.";
            return RedirectToAction(nameof(Listing));
        }
        var (success, message, hotelDto) = await _hotelService.GetByIdAsync(booking.RawHotelId, ct);
        if (!success || hotelDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }
        int nights = (int)(booking.CheckOut - booking.CheckIn).TotalDays;
        if (nights < 1) nights = 1;
        var paymentModel = new TravelBooking.Web.ViewModels.Payments.HotelPaymentViewModel
        {
            HotelId = hotelDto.Id,
            HotelName = booking.HotelName,
            Location = $"{hotelDto.City}, {hotelDto.Country}",
            CheckInDate = booking.CheckIn,
            CheckOutDate = booking.CheckOut,
            Nights = nights,
            Rooms = 1,
            Adults = booking.Guests,
            Children = 0,
            RoomType = booking.RoomType,
            PricePerNight = booking.TotalPrice / nights,
            Currency = booking.Currency ?? "TRY",
            GuestName = booking.ContactName,
            GuestEmail = booking.ContactEmail,
            GuestPhone = booking.ContactPhone ?? ""
        };
        return View("Payment", paymentModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletePayment(TravelBooking.Web.ViewModels.Payments.HotelPaymentViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return View("Payment", model);
        }

        // Check if user is authenticated (either as regular user or admin)
        var isAuthenticated = _authService.IsAuthenticated() || User.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
        {
            TempData["BookingError"] = "You must log in to make a reservation.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Booking), "Hotel") });
        }

        try
        {
            // Get user ID from auth service
            var userId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                TempData["BookingError"] = "Could not retrieve user information.";
                return RedirectToAction(nameof(Listing));
            }

            // Get hotel to know its original currency
            var (hotelSuccess, hotelMessage, hotelDto) = await _hotelService.GetByIdAsync(model.HotelId, ct);
            if (!hotelSuccess || hotelDto == null)
            {
                TempData["BookingError"] = "Could not retrieve hotel information.";
                return View("Payment", model);
            }

            // Get selected currency from cookie
            var selectedCurrency = _cookieHelper.GetCurrency();

            // model.Total already includes subtotal + 10% tax (HotelPaymentViewModel)
            decimal finalTotal = model.Total;

            // Parse currency
            if (!Enum.TryParse<Currency>(selectedCurrency, true, out var currency))
            {
                currency = Currency.TRY;
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

            //Örnek: Otel rezervasyonunda bilet yok; sadece odeme ve PNR ile rezervasyon kaydi olusturulur
            var createReservationDto = new CreateReservationDto
            {
                AppUserId = userId,
                TotalPrice = finalTotal,
                Currency = currency,
                Type = ReservationType.Hotel,
                HotelId = model.HotelId,
                PNR = GeneratePNR(),
                Tickets = new List<CreateTicketDto>(), // No tickets for hotel bookings
                Payment = payment
            };

            var (reservationSuccess, reservationMessage, reservationId) = await _reservationService.CreateAsync(createReservationDto, ct);
            
            if (!reservationSuccess || !reservationId.HasValue)
            {
                TempData["BookingError"] = $"Reservation could not be created: {reservationMessage}";
                return View("Payment", model);
            }

            HttpContext.Session.Remove("HotelPendingBooking");
            TempData["BookingSuccess"] = $"Your hotel reservation was created successfully! PNR: {createReservationDto.PNR}";
            return RedirectToAction("MyReservations", "Account");
        }
        catch (Exception ex)
        {
            TempData["BookingError"] = $"An error occurred: {ex.Message}";
            return View("Payment", model);
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
}
