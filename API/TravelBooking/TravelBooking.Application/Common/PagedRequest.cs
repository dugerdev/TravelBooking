namespace TravelBooking.Application.Common;

//---Sayfalama icin request modeli---//
public class PagedRequest
{
    public int PageNumber { get; set; } = 1;                  //---Sayfa numarasi (1'den baslar)---//
    public int PageSize { get; set; } = 10;                   //---Sayfa basina kayit sayisi---//

    //---Validation---//
    public int GetValidPageNumber()
    {
        return PageNumber < 1 ? 1 : PageNumber;
    }

    public int GetValidPageSize(int maxPageSize = 100)
    {
        if (PageSize < 1) return 10;
        return PageSize > maxPageSize ? maxPageSize : PageSize;
    }
}
