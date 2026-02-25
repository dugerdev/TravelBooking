namespace TravelBooking.Application.Common;

public class DataResult<T> : Result
{
    public T Data { get; }

    public DataResult(T data, bool success, string message = "") : base(success, message)
    {
        Data = data;
    }
}
