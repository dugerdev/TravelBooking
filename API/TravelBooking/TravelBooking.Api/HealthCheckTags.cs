namespace TravelBooking.Api;

//---Health check tag'leri icin static readonly field'lar---//
internal static class HealthCheckTags
{
    internal static readonly string[] Ready = { "ready" };
    internal static readonly string[] SelfAndLive = { "self", "live" };
}
