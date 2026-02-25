namespace TravelBooking.Web.DTOs.Admin;

public class ReservationStatisticsDto
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int CompletedReservations { get; set; }
    public Dictionary<string, int> ReservationsByStatus { get; set; } = new();
    public Dictionary<string, int> ReservationsByMonth { get; set; } = new();
}
