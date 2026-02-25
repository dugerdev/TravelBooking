namespace TravelBooking.Web.ViewModels;

/// <summary>
/// View model for the shared pagination partial.
/// </summary>
public class PaginationViewModel
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    /// <summary>Route values for filter preservation. Values are converted to string for asp-all-route-data.</summary>
    public Dictionary<string, string>? RouteValues { get; set; }
}
