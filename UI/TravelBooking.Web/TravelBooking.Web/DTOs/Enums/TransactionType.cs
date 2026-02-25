using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    Payment = 1,
    Refund = 2,
    Chargeback = 3
}
