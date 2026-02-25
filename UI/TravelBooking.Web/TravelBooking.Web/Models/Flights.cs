namespace TravelBooking.Web.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string? AirlineName { get; set; }
        public string? AircraftModel { get; set; } 
        public string? FlightNumber { get; set; }

        public string? DepartureCity { get; set; }
        public DateTime DepartureTime { get; set; }

        public string? ArrivalCity { get; set; }
        public DateTime ArrivalTime { get; set; } //

        public string? TotalDuration { get; set; } // "2h 30m" gibi
        public int StopCount { get; set; } // 0, 1, 2...
        public string? TransferCity { get; set; } // Aktarma sehri

        public decimal Price { get; set; }
        public string? BaggageAllowance { get; set; } // Orn: "25"

        public string? CabinClass { get; set; } // Orn: "Economy", "Business"
        public string?   PassengerType { get; set; } // Orn: "Adult", "Child"
        public int BaggageWeight { get; set; } // Orn: 20, 30
    }
}