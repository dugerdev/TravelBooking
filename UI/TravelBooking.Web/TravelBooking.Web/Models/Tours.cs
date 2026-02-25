namespace TravelBooking.Web.Models
{
    public class Tours
    {
        public int Id { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}