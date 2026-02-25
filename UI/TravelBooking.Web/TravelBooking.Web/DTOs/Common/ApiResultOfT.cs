namespace TravelBooking.Web.DTOs.Common;

public class ApiResult<T> : ApiResult
{
    public T? Data { get; set; }
}
