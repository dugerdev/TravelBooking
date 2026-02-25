using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod
{
    Card = 1,
    Cash = 2,
    PayPal = 3,
    BankTransfer = 4,
    Cryptocurrency = 5
}
