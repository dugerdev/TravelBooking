namespace TravelBooking.Application.Common;

public sealed class SuccessResult : Result
{
    public SuccessResult(string message = "") : base(true, message)
    {
    }
}
