using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    PaymentFailed = 4
}
