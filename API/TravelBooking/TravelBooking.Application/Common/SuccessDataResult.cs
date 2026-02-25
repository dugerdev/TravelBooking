namespace TravelBooking.Application.Common;

public sealed class SuccessDataResult<T> : DataResult<T>
{
    public SuccessDataResult(T data, string message = "") : base(data, true, message)
    {
    }
}
