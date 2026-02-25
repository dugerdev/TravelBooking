namespace TravelBooking.Web.Services.Email;

public interface IReservationEmailService
{
    /// <summary>
    /// Rezervasyon onayi e-postasi gonderir: "Biletiniz basariyla satin alindi" ve PNR kodu.
    /// </summary>
    Task SendFlightBookingConfirmationAsync(string toEmail, string passengerName, string pnr, decimal totalPrice, string currency, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detayli ucus bilgileriyle rezervasyon onayi e-postasi gonderir.
    /// </summary>
    Task SendDetailedFlightBookingConfirmationAsync(string toEmail, string passengerName, string pnr, 
        string flightNumber, string from, string to, DateTime departureDate, DateTime arrivalDate,
        decimal totalPrice, string currency, CancellationToken cancellationToken = default);
}
