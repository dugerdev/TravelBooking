using FluentValidation;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Validators;

//---Rezervasyon dogrulama kurallari---//
public class ReservationValidator : AbstractValidator<Reservation>
{
    public ReservationValidator()
    {
        RuleFor(x => x.PNR)
            .NotEmpty().WithMessage("PNR kodu zorunludur.")
            .MaximumLength(10).WithMessage("PNR kodu en fazla 10 karakter olabilir.");

        RuleFor(x => x.AppUserId)
            .NotEmpty().WithMessage("Kullanici kimligi zorunludur.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Toplam fiyat negatif olamaz.");

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Gecerli bir para birimi secilmelidir.");
    }
}

