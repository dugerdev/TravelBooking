namespace TravelBooking.Web.Models
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // Haber Basligi (Arama buraya bakacak)
        public string Content { get; set; } = string.Empty; // Haber Icerigi
        public string ImageUrl { get; set; } = string.Empty; // Haber Gorseli
        public DateTime CreatedDate { get; set; } // Yayinlanma Tarihi
    }
}