using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Flights;
using TravelBooking.Web.Services.Reservations;
using TravelBooking.Web.Services.Passengers;
using TravelBooking.Web.Services.Auth;
using TravelBooking.Web.Services.Airports;
using TravelBooking.Web.Services.Email;
using TravelBooking.Web.Services.Currency;
using TravelBooking.Web.Helpers;
using TravelBooking.Web.ViewModels.Flights;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.DTOs.Passengers;
using TravelBooking.Web.DTOs.Enums;
using TravelBooking.Web.DTOs.Flights;
using System.Globalization;

namespace TravelBooking.Web.Controllers;

public class FlightController : Controller
{
    private readonly IFlightService _flightService;
    private readonly IReservationService _reservationService;
    private readonly IPassengerService _passengerService;
    private readonly IAuthService _authService;
    private readonly IAirportService _airportService;
    private readonly IReservationEmailService _reservationEmailService;
    private readonly ICookieHelper _cookieHelper;
    private readonly ICurrencyService _currencyService;

    public FlightController(
        IFlightService flightService, 
        IReservationService reservationService,
        IPassengerService passengerService,
        IAuthService authService,
        IAirportService airportService,
        IReservationEmailService reservationEmailService,
        ICookieHelper cookieHelper,
        ICurrencyService currencyService)
    {
        _flightService = flightService;
        _reservationService = reservationService;
        _passengerService = passengerService;
        _authService = authService;
        _airportService = airportService;
        _reservationEmailService = reservationEmailService;
        _cookieHelper = cookieHelper;
        _currencyService = currencyService;
    }

    [HttpGet]
    public IActionResult SearchFlights(string? fromCity, string? toCity, string? departureDate, string? returnDate,
        int adultCount = 1, int childCount = 0, int infantCount = 0, bool directFlight = false,
        string? way = "one-way", string? cabinClass = "Economy")
    {
        return RedirectToAction(nameof(Listing), new
        {
            fromCity,
            toCity,
            departureDate,
            returnDate,
            adultCount,
            childCount,
            infantCount,
            directFlight,
            way,
            cabinClass
        });
    }

    [HttpGet]
    public async Task<IActionResult> FlightStatus(string? flightNumber, string? fromCity, string? toCity, string? date, CancellationToken ct = default)
    {
        var model = new ViewModels.Flights.FlightStatusViewModel
        {
            FlightNumber = flightNumber,
            FromCity = fromCity,
            ToCity = toCity,
            Date = date
        };

        if (!string.IsNullOrWhiteSpace(flightNumber))
        {
            // Search by flight number - SearchHybridAsync doesn't support flight number search
            // We'll search by date only and filter client-side
            DateTime? searchDate = DateTime.TryParse(date, out var d) ? d : null;
            var (success, message, flights) = await _flightService.SearchHybridAsync(null, null, searchDate, null, null, ct);
            if (success && flights != null && flights.Any())
            {
                // Filter by flight number client-side
                var filtered = flights.Where(f => f.FlightNumber?.Contains(flightNumber, StringComparison.OrdinalIgnoreCase) == true).ToList();
                model.Flights = filtered;
                if (!filtered.Any())
                {
                    model.ErrorMessage = "No results found for this flight number.";
                }
            }
            else
            {
                model.ErrorMessage = message ?? "Flight not found.";
            }
        }
        else if (!string.IsNullOrWhiteSpace(fromCity) && !string.IsNullOrWhiteSpace(toCity))
        {
            // Search by route
            DateTime? searchDate = DateTime.TryParse(date, out var d) ? d : null;
            var (success, message, flights) = await _flightService.SearchHybridAsync(fromCity, toCity, searchDate, null, null, ct);
            if (success && flights != null && flights.Any())
            {
                model.Flights = flights.ToList();
            }
            else
            {
                model.ErrorMessage = message ?? "No flights found for this route.";
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Listing(string? fromCity, string? toCity, string? departureDate, string? returnDate,
        int adultCount = 1, int childCount = 0, int infantCount = 0, bool directFlight = false,
        string? way = "one-way", string? cabinClass = "Economy",
        string[]? stops = null, string[]? departureTime = null, string[]? arrivalTime = null,
        string[]? duration = null, string[]? airlines = null,
        CancellationToken ct = default)
    {
        var search = new FlightSearchViewModel
        {
            FromCity = fromCity,
            ToCity = toCity,
            DepartureDate = departureDate,
            ReturnDate = returnDate,
            AdultCount = adultCount,
            ChildCount = childCount,
            InfantCount = infantCount,
            DirectFlight = directFlight,
            Way = way,
            CabinClass = cabinClass
        };

        var vm = new FlightListingViewModel { Search = search };
        int totalPax = adultCount + childCount + infantCount;
        ViewBag.TotalPassengers = totalPax > 0 ? totalPax : 1;

        if (string.IsNullOrWhiteSpace(fromCity) || string.IsNullOrWhiteSpace(toCity) || string.IsNullOrWhiteSpace(departureDate))
        {
            vm.InfoMessage = "Enter from, to and departure date to search for flights.";
            return View(vm);
        }

        void StoreFlightsInSession(List<TravelBooking.Web.DTOs.Flights.ExternalFlightDto> list)
        {
            if (list.Count > 0)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(list);
                HttpContext.Session.SetString("FlightListing", json);
            }
        }

        void StoreReturnFlightsInSession(List<TravelBooking.Web.DTOs.Flights.ExternalFlightDto> list)
        {
            if (list.Count > 0)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(list);
                HttpContext.Session.SetString("ReturnFlightListing", json);
                HttpContext.Session.SetString("Way", "round-trip");
            }
        }

        //Örnek: Ucus listesini aktarmalar, kalkis/varis saati, sure ve havayolu filtrelerine gore sinirlandirir
            IEnumerable<TravelBooking.Web.DTOs.Flights.ExternalFlightDto> ApplyFlightFilters(
            IEnumerable<TravelBooking.Web.DTOs.Flights.ExternalFlightDto> source,
            string[]? stops, string[]? departureTime, string[]? arrivalTime, string[]? duration, string[]? airlines)
        {
            var filtered = source;
            //Örnek: Aktarma sayisi filtresi (0: direkt, 1: bir aktarma, 2+: iki veya daha fazla)
            if (stops != null && stops.Length > 0)
            {
                filtered = filtered.Where(f =>
                {
                    var stopCount = f.FlightType == "Direct" ? 0 : 1;
                    return stops.Any(s =>
                    {
                        if (s == "0") return stopCount == 0;
                        if (s == "1") return stopCount == 1;
                        if (s == "2") return stopCount >= 2;
                        return false;
                    });
                });
            }
            //Örnek: Kalkis saati filtresi (morning: 00-12, afternoon: 12-18, evening: 18-24)
            if (departureTime != null && departureTime.Length > 0)
            {
                filtered = filtered.Where(f =>
                {
                    var hour = f.ScheduledDeparture.Hour;
                    return departureTime.Any(dt =>
                    {
                        if (dt == "morning") return hour >= 0 && hour < 12;
                        if (dt == "afternoon") return hour >= 12 && hour < 18;
                        if (dt == "evening") return hour >= 18 && hour < 24;
                        return false;
                    });
                });
            }
            //Örnek: Varis saati filtresi (morning/afternoon/evening)
            if (arrivalTime != null && arrivalTime.Length > 0)
            {
                filtered = filtered.Where(f =>
                {
                    var hour = f.ScheduledArrival.Hour;
                    return arrivalTime.Any(at =>
                    {
                        if (at == "morning") return hour >= 0 && hour < 12;
                        if (at == "afternoon") return hour >= 12 && hour < 18;
                        if (at == "evening") return hour >= 18 && hour < 24;
                        return false;
                    });
                });
            }
            //Örnek: Ucus suresi filtresi (0-5, 5-10, 10-15 saat veya 15+ saat)
            if (duration != null && duration.Length > 0)
            {
                filtered = filtered.Where(f =>
                {
                    var totalHours = (f.ScheduledArrival - f.ScheduledDeparture).TotalHours;
                    return duration.Any(d =>
                    {
                        if (d == "0-5") return totalHours >= 0 && totalHours < 5;
                        if (d == "5-10") return totalHours >= 5 && totalHours < 10;
                        if (d == "10-15") return totalHours >= 10 && totalHours < 15;
                        if (d == "15+") return totalHours >= 15;
                        return false;
                    });
                });
            }
            //Örnek: Havayolu adina gore filtreleme
            if (airlines != null && airlines.Length > 0)
                filtered = filtered.Where(f => airlines.Contains(f.AirlineName, StringComparer.OrdinalIgnoreCase));
            return filtered;
        }

        if (!DateTime.TryParse(departureDate, out var date))
        {
            vm.ErrorMessage = "Enter a valid departure date (e.g. 2026-02-15).";
            return View(vm);
        }

        if (date.Date < DateTime.Today)
        {
            vm.ErrorMessage = "Cannot search for flights in the past.";
            return View(vm);
        }

        var (success, message, flights) = await _flightService.SearchExternalAsync(fromCity!.Trim(), toCity!.Trim(), date, 20, ct);

        vm.Flights = flights != null ? ApplyFlightFilters(flights, stops, departureTime, arrivalTime, duration, airlines).ToList() : new List<TravelBooking.Web.DTOs.Flights.ExternalFlightDto>();
        StoreFlightsInSession(vm.Flights);

        // Round-trip: fetch return flights (toCity -> fromCity on returnDate)
        if (way == "round-trip" && !string.IsNullOrWhiteSpace(returnDate) && DateTime.TryParse(returnDate, out var returnDateParsed) && returnDateParsed.Date >= date.Date && returnDateParsed.Date >= DateTime.Today)
        {
            var (returnSuccess, returnMessage, returnFlights) = await _flightService.SearchExternalAsync(toCity!.Trim(), fromCity!.Trim(), returnDateParsed, 20, ct);
            vm.ReturnFlights = returnFlights != null ? ApplyFlightFilters(returnFlights, stops, departureTime, arrivalTime, duration, airlines).ToList() : new List<TravelBooking.Web.DTOs.Flights.ExternalFlightDto>();
            StoreReturnFlightsInSession(vm.ReturnFlights);
        }

        if (!success)
            vm.ErrorMessage = message;
        else if (vm.Flights.Count == 0 && flights.Count > 0)
            vm.InfoMessage = "No flights found matching your filters. Please try changing the filters.";
        else if (vm.Flights.Count == 0)
            vm.InfoMessage = message ?? "No flights found for this route and date.";
        else
            vm.InfoMessage = message;

        return View(vm);
    }

    [HttpGet]
    public IActionResult Booking(string? externalFlightId, int index = -1, int returnIndex = -1, int passengerCount = 1)
    {
        if (passengerCount < 1) passengerCount = 1;
        ViewBag.InitialPassengerCount = passengerCount;

        var json = HttpContext.Session.GetString("FlightListing");
        if (index >= 0 && !string.IsNullOrEmpty(json))
        {
            try
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<TravelBooking.Web.DTOs.Flights.ExternalFlightDto>>(json);
                var flight = list?.ElementAtOrDefault(index);
                if (flight != null)
                {
                    var bookVm = new FlightBookingViewModel
                    {
                        ExternalFlightId = flight.ExternalFlightId,
                        FlightNumber = flight.FlightNumber,
                        AirlineName = flight.AirlineName,
                        DepartureCity = flight.DepartureCity ?? flight.DepartureAirportIATA ?? "",
                        ArrivalCity = flight.ArrivalCity ?? flight.ArrivalAirportIATA ?? "",
                        DepartureAirportIATA = flight.DepartureAirportIATA,
                        ArrivalAirportIATA = flight.ArrivalAirportIATA,
                        ScheduledDeparture = flight.ScheduledDeparture,
                        ScheduledArrival = flight.ScheduledArrival,
                        Price = flight.BasePriceAmount > 0 ? flight.BasePriceAmount : 1250,
                        Currency = flight.Currency,
                        StopCount = flight.FlightType == "Direct" ? 0 : 1,
                        CabinClass = "Economy"
                    };

                    if (returnIndex >= 0)
                    {
                        var returnJson = HttpContext.Session.GetString("ReturnFlightListing");
                        if (!string.IsNullOrEmpty(returnJson))
                        {
                            var returnList = System.Text.Json.JsonSerializer.Deserialize<List<TravelBooking.Web.DTOs.Flights.ExternalFlightDto>>(returnJson);
                            var returnFlight = returnList?.ElementAtOrDefault(returnIndex);
                            if (returnFlight != null)
                            {
                                bookVm.IsRoundTrip = true;
                                bookVm.ReturnPrice = returnFlight.BasePriceAmount > 0 ? returnFlight.BasePriceAmount : 1250;
                                bookVm.ReturnFlightNumber = returnFlight.FlightNumber;
                                bookVm.ReturnAirlineName = returnFlight.AirlineName;
                                bookVm.ReturnDepartureCity = returnFlight.DepartureCity ?? returnFlight.DepartureAirportIATA ?? "";
                                bookVm.ReturnArrivalCity = returnFlight.ArrivalCity ?? returnFlight.ArrivalAirportIATA ?? "";
                                bookVm.ReturnScheduledDeparture = returnFlight.ScheduledDeparture;
                                bookVm.ReturnScheduledArrival = returnFlight.ScheduledArrival;
                            }
                        }
                    }

                    return View(bookVm);
                }
            }
            catch { /* ignore */ }
        }

        return RedirectToAction(nameof(Listing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InitiatePayment(FlightBookingStep1ViewModel model)
    {
        if (model?.Passengers == null || model.Passengers.Count == 0)
        {
            TempData["BookingError"] = "Enter at least one passenger information.";
            return RedirectToAction(nameof(Booking));
        }

        var first = model.Passengers.FirstOrDefault();
        if (first == null)
        {
            TempData["BookingError"] = "Passenger information not found.";
            return RedirectToAction(nameof(Booking));
        }

        decimal basePrice = model.BasePrice;
        decimal extrasTotal = model.Passengers.Sum(p => p.MealPrice + p.ExtraBaggagePrice);
        decimal totalPrice = (basePrice * model.PassengerCount) + extrasTotal;
        decimal taxAmount = totalPrice * 0.1m;
        decimal finalTotal = totalPrice + taxAmount;

        var paymentVm = new TravelBooking.Web.ViewModels.Payments.FlightPaymentViewModel
        {
            FlightId = Guid.Empty,
            From = model.DepartureCity ?? "",
            To = model.ArrivalCity ?? "",
            DepartureDate = model.ScheduledDeparture,
            ReturnDate = null,
            CabinClass = model.CabinClass ?? "Economy",
            AdultCount = model.PassengerCount,
            ChildCount = 0,
            InfantCount = 0,
            Price = basePrice,
            Currency = model.Currency ?? "TRY",
            PassengerName = $"{first.FirstName ?? ""} {first.LastName ?? ""}".Trim(),
            PassengerEmail = first.Email ?? "",
            PassengerPhone = first.Phone ?? ""
        };

        // Store booking for ComplateBooking (payment step)
        var json = System.Text.Json.JsonSerializer.Serialize(model);
        HttpContext.Session.SetString("FlightPendingBooking", json);

        return View("Payment", paymentVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletePayment(FlightPaymentFormViewModel paymentForm, CancellationToken ct = default)
    {
        CompleteBookingViewModel? model = null;
        var pendingJson = HttpContext.Session.GetString("FlightPendingBooking");
        if (!string.IsNullOrEmpty(pendingJson))
        {
            try
            {
                var step1 = System.Text.Json.JsonSerializer.Deserialize<FlightBookingStep1ViewModel>(pendingJson);
                if (step1 != null && paymentForm != null)
                {
                    model = new CompleteBookingViewModel
                    {
                        ExternalFlightId = step1.ExternalFlightId,
                        FlightId = step1.FlightId,
                        PassengerCount = step1.PassengerCount,
                        Passengers = step1.Passengers,
                        FlightNumber = step1.FlightNumber,
                        AirlineName = step1.AirlineName,
                        DepartureCity = step1.DepartureCity,
                        ArrivalCity = step1.ArrivalCity,
                        ScheduledDeparture = step1.ScheduledDeparture,
                        ScheduledArrival = step1.ScheduledArrival,
                        BasePrice = step1.BasePrice,
                        Currency = step1.Currency ?? "USD",
                        CabinClass = step1.CabinClass ?? "Economy",
                        PaymentMethod = paymentForm.PaymentMethod ?? "Card",
                        CardNumber = paymentForm.CardNumber ?? "",
                        CardHolderName = paymentForm.CardHolderName ?? "",
                        ExpiryDate = paymentForm.ExpiryDate ?? "",
                        CVV = paymentForm.CVV ?? ""
                    };
                }
            }
            catch { /* ignore */ }
        }

        if (model == null)
        {
            TempData["BookingError"] = "Reservation session not found. Please start again from passenger information.";
            return RedirectToAction(nameof(Listing));
        }

        if (string.IsNullOrEmpty(model.PaymentMethod) || (model.PaymentMethod.Equals("Card", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(model.CardNumber)))
        {
            TempData["BookingError"] = "Please enter payment information.";
            return RedirectToAction(nameof(Booking));
        }

        // Check if user is authenticated (either as regular user or admin)
        var isAuthenticated = _authService.IsAuthenticated() || User.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
        {
            TempData["BookingError"] = "You must log in to make a reservation.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Booking), "Flight") });
        }

        try
        {
            //Örnek: Oturum acik kullanici ID'si alinir
            var userId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                TempData["BookingError"] = "Could not retrieve user information.";
                return RedirectToAction(nameof(Listing));
            }

            //Örnek: Harici ucus kontrolu - FlightId yoksa veya ExternalFlightId doluysa dis API ucusu
            bool isExternalFlight = !model.FlightId.HasValue || !string.IsNullOrEmpty(model.ExternalFlightId);

            //Örnek: Cookie'den secili para birimi alinir
            var selectedCurrency = _cookieHelper.GetCurrency();

            //Örnek: Toplam fiyat hesaplanir (base + yolcu ekstralari + %10 vergi)
            decimal basePrice = model.BasePrice;
            decimal extrasTotal = model.Passengers.Sum(p => p.MealPrice + p.ExtraBaggagePrice);
            decimal totalPrice = (basePrice * model.PassengerCount) + extrasTotal;
            decimal taxAmount = totalPrice * 0.1m; // 10% tax
            decimal finalTotal = totalPrice + taxAmount;

            // Parse currency
            if (!Enum.TryParse<Currency>(selectedCurrency, true, out var currency))
            {
                currency = Currency.TRY;
            }

            //Örnek: Sadece ic (veritabani) ucuslar icin bilet olusturulur; harici ucuslarda bilet yok
            var tickets = new List<CreateTicketDto>();
            if (!isExternalFlight && model.FlightId.HasValue)
            {
                //Örnek: Yolcular olusturulur ve ID'leri toplanir
                var passengerIds = new List<Guid>();
                foreach (var passenger in model.Passengers)
                {
                    // Parse birth date
                    if (!DateTime.TryParseExact(passenger.BirthDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" }, 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate))
                    {
                        TempData["BookingError"] = $"Invalid date of birth format: {passenger.BirthDate}";
                        return RedirectToAction(nameof(Booking));
                    }

                    var createPassengerDto = new CreatePassengerDto
                    {
                        PassengerFirstName = passenger.FirstName,
                        PassengerLastName = passenger.LastName,
                        NationalNumber = passenger.NationalNumber,
                        PassportNumber = passenger.PassportNumber,
                        DateOfBirth = birthDate,
                        PassengerType = GetPassengerTypeByAge(birthDate)
                    };

                    var (success, message, passengerId) = await _passengerService.CreateAsync(createPassengerDto, ct);
                    if (!success || !passengerId.HasValue)
                    {
                        TempData["BookingError"] = $"Could not create passenger: {message}";
                        return RedirectToAction(nameof(Booking));
                    }

                    passengerIds.Add(passengerId.Value);
                }

                // Create tickets for internal flights
                for (int i = 0; i < passengerIds.Count; i++)
                {
                    var passenger = model.Passengers[i];
                    var ticket = new CreateTicketDto
                    {
                        FlightId = model.FlightId.Value,
                        PassengerId = passengerIds[i],
                        Email = passenger.Email,
                        ContactPhoneNumber = passenger.Phone,
                        SeatClass = ParseSeatClass(model.CabinClass),
                        BaggageOption = passenger.ExtraBaggagePrice > 0 ? BaggageOption.Heavy : BaggageOption.Light,
                        TicketPrice = basePrice + passenger.MealPrice,
                        BaggageFee = passenger.ExtraBaggagePrice,
                        SeatNumber = null // Will be assigned by system
                    };
                    tickets.Add(ticket);
                }
            }

            //Örnek: Harici ucusta bilet olmadigi icin yolcular Participants olarak gonderilir, rezervasyon detayda listelenir
            var participants = new List<TravelBooking.Web.DTOs.Passengers.CreatePassengerDto>();
            if (isExternalFlight && model.Passengers != null && model.Passengers.Count > 0)
            {
                foreach (var p in model.Passengers)
                {
                    if (!DateTime.TryParseExact(p.BirthDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" },
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate))
                        birthDate = DateTime.Today.AddYears(-30);
                    participants.Add(new TravelBooking.Web.DTOs.Passengers.CreatePassengerDto
                    {
                        PassengerFirstName = p.FirstName,
                        PassengerLastName = p.LastName,
                        NationalNumber = p.NationalNumber ?? "",
                        PassportNumber = p.PassportNumber ?? "",
                        DateOfBirth = birthDate,
                        PassengerType = GetPassengerTypeByAge(birthDate)
                    });
                }
            }

            //Örnek: Odeme kaydi olusturulur (tutarlar, para birimi, transaction ID)
            var payment = new CreatePaymentDto
            {
                TransactionAmount = finalTotal,
                Currency = currency,
                PaymentMethod = ParsePaymentMethod(model.PaymentMethod),
                TransactionId = GenerateTransactionId(),
                TransactionType = TransactionType.Payment
            };

            //Örnek: Rezervasyon olusturulur (PNR, biletler, participants, odeme bilgisi)
            var createReservationDto = new CreateReservationDto
            {
                AppUserId = userId,
                TotalPrice = finalTotal,
                Currency = currency,
                Type = ReservationType.Flight,
                PNR = GeneratePNR(),
                Tickets = tickets,
                Participants = participants,
                FlightRouteSummary = isExternalFlight ? $"{model.DepartureCity ?? ""} → {model.ArrivalCity ?? ""}".Trim() : null,
                Payment = payment
            };

            var (reservationSuccess, reservationMessage, reservationId) = await _reservationService.CreateAsync(createReservationDto, ct);
            
            if (!reservationSuccess || !reservationId.HasValue)
            {
                TempData["BookingError"] = $"Reservation could not be created: {reservationMessage}";
                return RedirectToAction(nameof(Booking));
            }

            // Rezervasyon onay e-postasi: yolcunun girdigi mail adresine PNR ile "biletiniz basariyla satin alindi"
            var firstPassenger = model.Passengers.FirstOrDefault();
            var toEmail = firstPassenger?.Email?.Trim();
            if (!string.IsNullOrEmpty(toEmail))
            {
                var passengerName = $"{firstPassenger!.FirstName} {firstPassenger.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(passengerName)) passengerName = "Dear Passenger";
                await _reservationEmailService.SendFlightBookingConfirmationAsync(
                    toEmail, passengerName, createReservationDto.PNR, finalTotal, model.Currency ?? "TRY", ct);
            }

            HttpContext.Session.Remove("FlightPendingBooking");

            TempData["BookingSuccess"] = $"Your reservation was created successfully! PNR: {createReservationDto.PNR}";
            return RedirectToAction("MyReservations", "Account");
        }
        catch (Exception ex)
        {
            TempData["BookingError"] = $"An error occurred: {ex.Message}";
            return RedirectToAction(nameof(Booking));
        }
    }

    //Örnek: Dogum tarihine gore yolcu tipi belirler (Infant: 0-1, Child: 2-11, Adult: 12+)
    private static string GetPassengerTypeByAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        if (age < 2) return "Infant";
        if (age < 12) return "Child";
        return "Adult";
    }

    private SeatClass ParseSeatClass(string cabinClass)
    {
        return cabinClass?.ToLower() switch
        {
            "economy" => SeatClass.Economy,
            "business" => SeatClass.Business,
            "first class" => SeatClass.FirstClass,
            "premium economy" => SeatClass.PremiumEconomy,
            _ => SeatClass.Economy
        };
    }

    private PaymentMethod ParsePaymentMethod(string method)
    {
        return method?.ToLower() switch
        {
            "card" => PaymentMethod.Card,
            "cash" => PaymentMethod.Cash,
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageBooking(string bookingReference, string lastName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(bookingReference) || string.IsNullOrWhiteSpace(lastName))
        {
            TempData["ErrorMessage"] = "Please enter both booking reference and last name.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: PNR ile API'den rezervasyon bilgisi cekilir
        var (success, message, reservation) = await _reservationService.GetByPNRAsync(bookingReference.Trim().ToUpper(), ct);

        if (!success || reservation == null)
        {
            TempData["ErrorMessage"] = message ?? "Booking not found. Please check your booking reference and last name.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: Soyad eslestirmesi (guvenlik icin buyuk/kucuk harf duyarsiz)
        var passengerLastName = reservation.Passengers?.FirstOrDefault()?.LastName?.Trim().ToLower();
        if (passengerLastName != lastName.Trim().ToLower())
        {
            TempData["ErrorMessage"] = "Last name does not match our records.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: Rezervasyon yonetim ekrani icin ViewModel hazirlanir
        var viewModel = new ManageBookingViewModel
        {
            Reservation = reservation,
            PNR = bookingReference.ToUpper()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(string bookingReference, string lastName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(bookingReference) || string.IsNullOrWhiteSpace(lastName))
        {
            TempData["ErrorMessage"] = "Please enter both booking reference and last name.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: PNR ile rezervasyon cekilir
        var (success, message, reservation) = await _reservationService.GetByPNRAsync(bookingReference.Trim().ToUpper(), ct);

        if (!success || reservation == null)
        {
            TempData["ErrorMessage"] = message ?? "Booking not found. Please check your booking reference and last name.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: View icin rezervasyona ucus ve yolcu bilgileri eklenir
        reservation.Payment = new ReservationPaymentInfo { PaymentMethod = reservation.PaymentMethod, Status = reservation.PaymentStatus };
        reservation.Passengers = reservation.Tickets?.Select(t => t.Passenger).Where(p => p != null).Cast<PassengerDto>().ToList() ?? new List<PassengerDto>();
        if (reservation.Tickets?.Any() == true && reservation.Tickets[0].FlightId != Guid.Empty)
        {
            var (flightOk, _, flight) = await _flightService.GetByIdAsync(reservation.Tickets[0].FlightId, ct);
            if (flightOk && flight != null)
                reservation.Flight = flight;
        }

        //Örnek: Soyad dogrulamasi yapilir
        var passengerLastName = reservation.Passengers?.FirstOrDefault()?.LastName?.Trim().ToLower();
        if (passengerLastName != lastName.Trim().ToLower())
        {
            TempData["ErrorMessage"] = "Last name does not match our records.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: Sadece onaylanmis rezervasyonlar check-in yapabilir
        if (reservation.Status != ReservationStatus.Confirmed)
        {
            TempData["ErrorMessage"] = "Only confirmed reservations can be checked in.";
            return RedirectToAction(nameof(Listing));
        }

        //Örnek: Koltuk atanmamis biletlere otomatik koltuk numarasi atanir (ornegin 12A, 15C)
        if (reservation.Tickets != null && reservation.Tickets.Any())
        {
            var seatsAssigned = new List<string>();
            var seatRows = new[] { "A", "B", "C", "D", "E", "F" };
            var startRow = Random.Shared.Next(10, 30);

            for (int i = 0; i < reservation.Tickets.Count; i++)
            {
                var ticket = reservation.Tickets[i];
                if (string.IsNullOrWhiteSpace(ticket.SeatNumber))
                {
                    //Örnek: Koltuk numarasi uret (sira + koltuk harfi)
                    var seatNumber = $"{startRow + (i / 6)}{seatRows[i % 6]}";
                    seatsAssigned.Add(seatNumber);
                    
                    // Call API to assign seat
                    // Note: This would require the ticket ID which we need to get from the reservation
                    // For now, we'll just display the assigned seats
                }
                else
                {
                    seatsAssigned.Add(ticket.SeatNumber);
                }
            }

            //Örnek: Check-in onay sayfasi icin ViewModel olusturulur
            var viewModel = new CheckInViewModel
            {
                Reservation = reservation,
                PNR = bookingReference.ToUpper(),
                AssignedSeats = seatsAssigned,
                CheckInTime = DateTime.UtcNow
            };

            return View("CheckInConfirmation", viewModel);
        }

        TempData["ErrorMessage"] = "No tickets found for this reservation.";
        return RedirectToAction(nameof(Listing));
    }

    [HttpGet]
    public async Task<IActionResult> SearchAirports(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Json(new { success = false, data = new List<object>() });

        var (success, _, airports) = await _airportService.SearchAsync(query, 10, ct);
        
        if (!success || airports == null || airports.Count == 0)
            return Json(new { success = false, data = new List<object>() });

        var results = airports.Select(a => new
        {
            id = a.Id,
            iata = a.IATA_Code,
            city = a.City,
            country = a.Country,
            name = a.Name,
            display = $"{a.City}, {a.Country} ({a.IATA_Code}) - {a.Name}"
        }).ToList();

        return Json(new { success = true, data = results });
    }
}
