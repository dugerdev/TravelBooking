using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

/// <summary>
/// Profil guncelleme icin DTO dogrulama kurallari
/// Bu validator, UpdateProfileDto nesnesinin gecerli olup olmadigini kontrol eder
/// Tum alanlar opsiyoneldir, sadece gonderilen alanlar validate edilir
/// </summary>
public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        // Kullanici adi verilmisse, en az 3 en fazla 50 karakter olabilir
        RuleFor(x => x.UserName)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Kullanici adi en az 3 karakter olmalidir.")
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Kullanici adi en fazla 50 karakter olabilir.");

        // E-posta adresi verilmisse, gecerli bir e-posta formatinda olmalidir
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Gecerli bir e-posta adresi giriniz.");

        // Telefon numarasi verilmisse, uluslararasi formatta olmalidir
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Gecerli bir telefon numarasi formati giriniz.");
    }
}

