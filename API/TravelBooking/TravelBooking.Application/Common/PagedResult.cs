namespace TravelBooking.Application.Common;

//---Sayfalama sonuc modeli---//
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }                       //---Toplam kayit sayisi---//
    public int PageNumber { get; set; }                       //---Mevcut sayfa numarasi---//
    public int PageSize { get; set; }                         //---Sayfa basina kayit sayisi---//
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult()
    {
    }

    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
