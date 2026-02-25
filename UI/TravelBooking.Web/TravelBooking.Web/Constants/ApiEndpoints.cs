namespace TravelBooking.Web.Constants;

/// <summary>
/// Centralized API endpoint paths. All UI services should use these constants instead of hardcoded strings.
/// </summary>
public static class ApiEndpoints
{
    // Auth
    public const string Auth = "api/auth";
    public static string AuthLogin => $"{Auth}/login";
    public static string AuthSignup => $"{Auth}/signup";
    public static string AuthLogout => $"{Auth}/logout";
    public static string AuthRefresh => $"{Auth}/refresh";
    public static string AuthForgotPassword => $"{Auth}/forgot-password";
    public static string AuthResetPassword => $"{Auth}/reset-password";

    // Account
    public const string Account = "api/account";
    public static string AccountProfile => $"{Account}/profile";
    public static string AccountChangePassword => $"{Account}/change-password";
    public static string AccountReservations(int pageNumber, int pageSize) => $"{Account}/reservations?PageNumber={pageNumber}&PageSize={pageSize}";

    // Flights
    public const string Flights = "api/Flights";
    public static string FlightById(Guid id) => $"{Flights}/{id}";
    public static string FlightsSearchExternal(string from, string to, DateTime date, int limit = 20) =>
        $"{Flights}/search-external?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}&date={date:yyyy-MM-dd}&limit={limit}";
    public static string FlightsSearch(string? queryString) => string.IsNullOrEmpty(queryString) ? $"{Flights}/search" : $"{Flights}/search?{queryString.TrimStart('&')}";

    // Airports
    public const string Airports = "api/Airports";
    public static string AirportById(Guid id) => $"{Airports}/{id}";
    public static string AirportsSearch(string query, int limit = 20) => $"{Airports}/search?query={Uri.EscapeDataString(query)}&limit={limit}";

    // Hotels
    public const string Hotels = "api/Hotels";
    public static string HotelById(Guid id) => $"{Hotels}/{id}";
    public static string HotelsPaged(int pageNumber, int pageSize) => $"{Hotels}?PageNumber={pageNumber}&PageSize={pageSize}";
    public static string HotelsSearch(string? queryString) => string.IsNullOrEmpty(queryString) ? $"{Hotels}/search" : $"{Hotels}/search?{queryString.TrimStart('&')}";

    // Cars
    public const string Cars = "api/Cars";
    public static string CarById(Guid id) => $"{Cars}/{id}";
    public static string CarsPaged(int pageNumber, int pageSize) => $"{Cars}?PageNumber={pageNumber}&PageSize={pageSize}";
    public static string CarsSearch(string? queryString) => string.IsNullOrEmpty(queryString) ? $"{Cars}/search" : $"{Cars}/search?{queryString.TrimStart('&')}";

    // Tours
    public const string Tours = "api/Tours";
    public static string TourById(Guid id) => $"{Tours}/{id}";
    public static string ToursPaged(int pageNumber, int pageSize) => $"{Tours}?PageNumber={pageNumber}&PageSize={pageSize}";
    public static string ToursSearch(string? queryString) => string.IsNullOrEmpty(queryString) ? $"{Tours}/search" : $"{Tours}/search?{queryString.TrimStart('&')}";

    // News
    public const string News = "api/News";
    public static string NewsById(Guid id) => $"{News}/{id}";
    public static string NewsPaged(int pageNumber, int pageSize) => $"{News}?PageNumber={pageNumber}&PageSize={pageSize}";
    public static string NewsSearch(string? queryString) => string.IsNullOrEmpty(queryString) ? $"{News}/search" : $"{News}/search?{queryString.TrimStart('&')}";

    // Reservations
    public const string Reservations = "api/Reservations";
    public static string ReservationsList => Reservations;
    public static string ReservationById(Guid id) => $"{Reservations}/{id}";
    public static string ReservationByPnr(string pnr) => $"{Reservations}/pnr/{pnr}";
    public static string ReservationCancel(Guid id) => $"{Reservations}/{id}/cancel";

    // Passengers
    public const string Passengers = "api/Passengers";
    public static string PassengerById(Guid id) => $"{Passengers}/{id}";

    // Contact Messages
    public const string ContactMessages = "api/ContactMessages";
    public static string ContactMessageById(Guid id) => $"{ContactMessages}/{id}";
    public static string ContactMessageMarkRead(Guid id) => $"{ContactMessages}/{id}/mark-read";
    public static string ContactMessageResponse(Guid id) => $"{ContactMessages}/{id}/response";
    public static string ContactMessagesUnreadCount => $"{ContactMessages}/unread-count";

    // Admin - Statistics
    public static string AdminStatisticsDashboard => "api/admin/statistics/dashboard";
    public static string AdminStatisticsRevenue(string? queryString) => string.IsNullOrEmpty(queryString) ? "api/admin/statistics/revenue" : $"api/admin/statistics/revenue?{queryString.TrimStart('&')}";
    public static string AdminStatisticsReservations => "api/admin/statistics/reservations";

    // Admin - Users
    public static string AdminUsers(int pageNumber, int pageSize) => $"api/admin/users?PageNumber={pageNumber}&PageSize={pageSize}";
    public static string AdminUserById(Guid id) => $"api/admin/users/{id}";
    public static string AdminUserById(string id) => $"api/admin/users/{id}";
    public static string AdminUserLock(string id) => $"api/admin/users/{id}/lock";
    public static string AdminUserUnlock(string id) => $"api/admin/users/{id}/unlock";

    // Admin - Flights
    public static string AdminFlights => "api/admin/flights";
    public static string AdminFlightsPaged(int pageNumber, int pageSize) => $"api/admin/flights?pageNumber={pageNumber}&pageSize={pageSize}";
    public static string AdminFlightById(Guid id) => $"api/admin/flights/{id}";
    public static string AdminFlightsCreate => "api/flights";

    // Admin - Airports (lowercase in API)
    public static string AdminAirports => "api/airports";
    public static string AdminAirportById(Guid id) => $"api/airports/{id}";

    // Admin - Reservations
    public static string AdminReservations(string? status = null) =>
        string.IsNullOrEmpty(status) ? "api/admin/reservations" : $"api/admin/reservations?status={status}";
    public static string AdminReservationsPaged(int pageNumber, int pageSize, string? status = null)
    {
        var qs = $"pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(status)) qs += $"&status={status}";
        return $"api/admin/reservations?{qs}";
    }
    public static string AdminReservationById(Guid id) => $"api/admin/reservations/{id}";
    public static string AdminReservationCancel(Guid id) => $"api/admin/reservations/{id}/cancel";

    // Admin - Testimonials
    public static string TestimonialsApproved => "api/testimonials/approved";
    public static string AdminTestimonials => "api/admin/testimonialsadmin";
    public static string AdminTestimonialsPending => "api/admin/testimonialsadmin/pending";
    public static string AdminTestimonialApprove(Guid id) => $"api/admin/testimonialsadmin/{id}/approve";
    public static string AdminTestimonialReject(Guid id) => $"api/admin/testimonialsadmin/{id}/reject";
    public static string AdminTestimonialDelete(Guid id) => $"api/admin/testimonialsadmin/{id}";
    public static string AdminTestimonialsBulkApprove => "api/admin/testimonialsadmin/bulk-approve";
}
