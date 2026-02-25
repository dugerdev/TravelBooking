namespace TravelBooking.Web.ViewModels.Stripe;

public class StripeCheckoutRequest
{
    /// <summary>Amount in smallest currency unit (e.g. kurus for TRY, cents for USD).</summary>
    public long AmountKurus { get; set; }
    public string Currency { get; set; } = "try";
    public string ProductName { get; set; } = "TravelBooking Rezervasyon";
}
