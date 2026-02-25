namespace TravelBooking.Web.ViewModels.Payments;

/// <summary>
/// Shared interface for payment form partial - used by Tour, Flight, Hotel, Car payment views.
/// </summary>
public interface IPaymentFormViewModel
{
    string PaymentMethod { get; set; }
    string CardNumber { get; set; }
    string CardHolderName { get; set; }
    string ExpiryDate { get; set; }
    string CVV { get; set; }
    string BillingAddress { get; set; }
    string City { get; set; }
    string Country { get; set; }
    string PostalCode { get; set; }
    bool AcceptTerms { get; set; }
}
