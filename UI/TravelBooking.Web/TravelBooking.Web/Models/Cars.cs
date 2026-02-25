namespace TravelBooking.Web.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string CarBrand { get; set; } = string.Empty; // Marka
        public string CarModel { get; set; } = string.Empty; // Model (Arama buraya bakacak)
        public string Transmission { get; set; } = string.Empty; // Vites Tipi
        public decimal DailyPrice { get; set; } // Gunluk Ucret
        public string ImageUrl { get; set; } = string.Empty; // Araba Gorseli
    }
}