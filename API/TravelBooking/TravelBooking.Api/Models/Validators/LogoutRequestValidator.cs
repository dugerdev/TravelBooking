using FluentValidation;
using TravelBooking.Api.Models;

namespace TravelBooking.Api.Models.Validators;

public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
