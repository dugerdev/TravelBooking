namespace TravelBooking.Application.Common;

public sealed class ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult(T data, string message = "") : base(data, false, message)
    {
    }
}
