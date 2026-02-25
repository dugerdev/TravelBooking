namespace TravelBooking.Application.Common;

//---Filtreleme icin base model---//
public class FilterRequest : PagedRequest
{
    public string? SearchTerm { get; set; }                   //---Genel arama terimi---//
    public string? SortBy { get; set; }                       //---Siralama alani---//
    public bool SortDescending { get; set; } = false;         //---Azalan siralama mi?---//
}
