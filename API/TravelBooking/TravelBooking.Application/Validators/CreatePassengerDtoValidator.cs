using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

/// <summary>
/// Yolcu olusturma icin DTO dogrulama kurallari
/// Bu validator, CreatePassengerDto nesnesinin gecerli olup olmadigini kontrol eder
/// </summary>
public class CreatePassengerDtoValidator : AbstractValidator<CreatePassengerDto>
{
    public CreatePassengerDtoValidator()
    {
        // Yolcu adi zorunludur ve en fazla 50 karakter olabilir
        RuleFor(x => x.PassengerFirstName)
            .NotEmpty().WithMessage("Yolcu adi zorunludur.")
            .MaximumLength(50).WithMessage("Yolcu adi en fazla 50 karakter olabilir.");

        // Yolcu soyadi zorunludur ve en fazla 50 karakter olabilir
        RuleFor(x => x.PassengerLastName)
            .NotEmpty().WithMessage("Yolcu soyadi zorunludur.")
            .MaximumLength(50).WithMessage("Yolcu soyadi en fazla 50 karakter olabilir.");

        // TC Kimlik numarasi zorunlu degil ama verilmisse en fazla 20 karakter olabilir
        RuleFor(x => x.NationalNumber)
            .MaximumLength(20).WithMessage("TC Kimlik numarasi en fazla 20 karakter olabilir.");

        // Pasaport numarasi zorunlu degil ama verilmisse en fazla 20 karakter olabilir
        RuleFor(x => x.PassportNumber)
            .MaximumLength(20).WithMessage("Pasaport numarasi en fazla 20 karakter olabilir.");

        // Dogum tarihi zorunludur ve gelecek bir tarih olamaz
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Dogum tarihi zorunludur.")
            .LessThan(DateTime.UtcNow).WithMessage("Dogum tarihi gelecek bir tarih olamaz.");

        // Yolcu tipi enum degerlerinden biri olmalidir (Adult, Child, Infant gibi)
        RuleFor(x => x.PassengerType)
            .IsInEnum().WithMessage("Gecerli bir yolcu tipi secilmelidir.");
    }
}

