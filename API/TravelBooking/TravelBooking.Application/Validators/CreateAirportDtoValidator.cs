using FluentValidation;
using TravelBooking.Application.Dtos;
using System.Text.RegularExpressions;

namespace TravelBooking.Application.Validators;

/// <summary>
/// Havalimani olusturma icin DTO dogrulama kurallari
/// Bu validator, CreateAirportDto nesnesinin gecerli olup olmadigini kontrol eder
/// </summary>
public class CreateAirportDtoValidator : AbstractValidator<CreateAirportDto>
{
    public CreateAirportDtoValidator()
    {
        // IATA kodu zorunludur, tam olarak 3 karakter olmali ve sadece buyuk harf icermelidir
        // Ornek: IST, ESB, JFK
        RuleFor(x => x.IATA_Code)
            .NotEmpty().WithMessage("IATA kodu zorunludur.")
            .Length(3).WithMessage("IATA kodu tam olarak 3 karakter olmalidir.")
            .Matches("^[A-Z]{3}$").WithMessage("IATA kodu 3 buyuk harf olmalidir.");

        // Sehir adi zorunludur ve en fazla 100 karakter olabilir
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Sehir adi zorunludur.")
            .MaximumLength(100).WithMessage("Sehir adi en fazla 100 karakter olabilir.");

        // Ulke adi zorunludur ve en fazla 100 karakter olabilir
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ulke adi zorunludur.")
            .MaximumLength(100).WithMessage("Ulke adi en fazla 100 karakter olabilir.");

        // Havalimani adi zorunludur ve en fazla 200 karakter olabilir
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Havalimani adi zorunludur.")
            .MaximumLength(200).WithMessage("Havalimani adi en fazla 200 karakter olabilir.");
    }
}

