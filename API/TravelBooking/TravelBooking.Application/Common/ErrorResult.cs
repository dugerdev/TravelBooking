namespace TravelBooking.Application.Common;

public sealed class ErrorResult : Result
{
    public ErrorResult(string message = "") : base(false, message)
    {
    }
}
