using FluentValidation;
using TravelBooking.Domain.Entities;
using System.Text.RegularExpressions;

namespace TravelBooking.Application.Validators;

//---Havalimani dogrulama kurallari---//
public class AirportValidator : AbstractValidator<Airport>
{
    public AirportValidator()
    {
        RuleFor(x => x.IATA_Code)
            .NotEmpty().WithMessage("IATA kodu zorunludur.")
            .Length(3).WithMessage("IATA kodu tam olarak 3 karakter olmalidir.")
            .Matches("^[A-Z]{3}$").WithMessage("IATA kodu 3 buyuk harf olmalidir.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Sehir adi zorunludur.")
            .MaximumLength(100).WithMessage("Sehir adi en fazla 100 karakter olabilir.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ulke adi zorunludur.")
            .MaximumLength(100).WithMessage("Ulke adi en fazla 100 karakter olabilir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Havalimani adi zorunludur.")
            .MaximumLength(200).WithMessage("Havalimani adi en fazla 200 karakter olabilir.");
    }
}

