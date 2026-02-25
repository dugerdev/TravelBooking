using FluentValidation;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Validators;

//---Yolcu dogrulama kurallari---//
public class PassengerValidator : AbstractValidator<Passenger>
{
    public PassengerValidator()
    {
        RuleFor(x => x.PassengerFirstName)
            .NotEmpty().WithMessage("Yolcu adi zorunludur.")
            .MaximumLength(50).WithMessage("Yolcu adi en fazla 50 karakter olabilir.");

        RuleFor(x => x.PassengerLastName)
            .NotEmpty().WithMessage("Yolcu soyadi zorunludur.")
            .MaximumLength(50).WithMessage("Yolcu soyadi en fazla 50 karakter olabilir.");

        RuleFor(x => x.NationalNumber)
            .MaximumLength(20).WithMessage("TC Kimlik numarasi en fazla 20 karakter olabilir.");

        RuleFor(x => x.PassportNumber)
            .MaximumLength(20).WithMessage("Pasaport numarasi en fazla 20 karakter olabilir.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Dogum tarihi zorunludur.")
            .LessThan(DateTime.UtcNow).WithMessage("Dogum tarihi gelecek bir tarih olamaz.");
    }
}

